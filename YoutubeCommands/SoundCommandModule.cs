using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Web;
using bzbotDiscordNet;
using bzbotDiscordNet.DownloadSystem;
using BzBotDiscordNet.GuildDB;
using bzbotDiscordNet.YoutubeApi;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BzBotDiscordNet.YoutubeCommands;

public class SoundCommandModule(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    SongDataService songDataService,
    ILogger<SoundCommandModule> logger,
    GuildDbEntry guildDbEntry)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("play", "Playlist or Youtube video link")]
    private async Task PlayListAsync(string url) {
        var user = Context.User as SocketGuildUser;
        var userVoiceChannel = user?.VoiceChannel;
        var userInChannel = user?.VoiceChannel is not null;
        var guildId = Context.Guild.Id;
        var botSessionActive = songDataService.GetGuildSessionData(guildId).ActiveSession;
        var inSameChannel = userVoiceChannel?.Id == songDataService.GetGuildSessionData(guildId).CurrentChannelId;

        if (!userInChannel) {
            await RespondAsync($"You need to be in a voice channel to use this command.", ephemeral: true);
            return;
        }

        if (botSessionActive && !inSameChannel) {
            await RespondAsync(
                $"You need to be in the same channel as bot to use this command.{userVoiceChannel?.Id} != {songDataService.GetGuildSessionData(guildId).CurrentChannelId}",
                ephemeral: true);
            return;
        }

        if (!Extensions.IsValidUrl(url)) {
            await RespondAsync("Enter valid Youtube URL");
            return;
        }
        
        songDataService.GetGuildSessionData(guildId).DownloadCancellationTokenSource =
            new CancellationTokenSource();
        var cancellationToken = songDataService.GetGuildSessionData(guildId).DownloadCancellationTokenSource.Token;
        
        if (Extensions.IsPlaylist(url)) {
            await RespondAsync($"Downloading from playlist: {url}", ephemeral: true);
            var apiKey = configuration.GetValue<string>("YoutubeApiKey");
            var songList = await PlaylistHelper.VideoUrlListFromPlaylist(url, apiKey);
            songDataService.AddPlaylistToDownloadList(songList, Context.Guild.Id);
            await StartDownloadAndPlay(userVoiceChannel,cancellationToken);
        }
        else if (Extensions.IsVideo(url)) {
            await RespondAsync("Downloading from {url}", ephemeral: true);
            songDataService.AddUrlToDownloadList(url, Context.Guild.Id);
           
            await StartDownloadAndPlay(userVoiceChannel,cancellationToken);
        }
    }

    private async Task StartDownloadAndPlay(SocketVoiceChannel? channel, CancellationToken cancellationToken) {
            var guildId = Context.Guild.Id;
            if (songDataService.GetGuildSessionData(guildId).IsDownloading)
                return;

            while (songDataService.ContinueDownload(guildId)) {
                songDataService.GetGuildSessionData(guildId).IsDownloading = true;
                var (success, url) = songDataService.DequeueUrlFromDownloadListToDownload(guildId);
                if (!success) continue;
                var fetch = await YoutubeDownload.FetchVideoData(url);
                var filePath = await DownloadCheck(guildId, url, fetch, cancellationToken);
                if (filePath == string.Empty) {
                    logger.LogInformation("Can't download, skipping");
                    songDataService.GetGuildSessionData(guildId).IsDownloading = false;
                    await Context.Channel.SendMessageAsync("Error downloading");
                    continue;
                }
                if (filePath == "cancelled") {
                    await Context.Channel.SendMessageAsync("Downloading cancelled for previous commands.");
                    return;
                } 
                
                var newSong = new SongData(fetch.id, fetch.title, filePath, url);

                Console.WriteLine(filePath);
                if (songDataService.GetGuildSessionData(guildId).ActiveSession) {
                    AddToQueue(newSong);
                    continue;
                }

                _ = AddToQueueAndPlay(newSong, channel);
            }
            songDataService.GetGuildSessionData(guildId).IsDownloading = false;
    }
    
    private async Task AddToQueueAndPlay(SongData songData, SocketVoiceChannel? channel) {
        var guildId = Context.Guild.Id;
        songDataService.GetGuildSessionData(guildId).SongDatas.Add(songData);
        var audioClient = await ConnectToChannel(channel);
        await PlayNext(audioClient);
    }

    private void AddToQueue(SongData songData) {
        var guildId = Context.Guild.Id;
        songDataService.GetGuildSessionData(guildId).SongDatas.Add(songData);
        logger.LogInformation($"Added to queue: {songData.urlId}");
    }

    private async Task<IAudioClient?> ConnectToChannel(SocketVoiceChannel? channel) {
        var guildId = Context.Guild.Id;
        if (songDataService.GetGuildSessionData(guildId).IsPlaying) {
            return songDataService.GetGuildSessionData(guildId).AudioClient;
        }
        var audioClient = await channel.ConnectAsync();
        songDataService.GetGuildSessionData(guildId).ActiveSession = true;
        songDataService.GetGuildSessionData(guildId).CurrentChannelId = channel.Id;
        songDataService.GetGuildSessionData(guildId).AudioClient = audioClient;
        return audioClient;
    }

    private async Task PlayNext(IAudioClient? audioClient) {
        var guildId = Context.Guild.Id;
        var sessionData = songDataService.GetGuildSessionData(guildId);
        
        if (sessionData.IsPlaying || sessionData.SongDatas.Count == 0)
            return;
        songDataService.GetGuildSessionData(guildId).IsPlaying = true;

        sessionData.PlaybackCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = sessionData.PlaybackCancellationTokenSource.Token;
        
        Console.WriteLine($"Playing = {sessionData.IsPlaying}");
        try {
            var filePath =
                sessionData.SongDatas[sessionData.CurrentSongIndex].FilePath;

            await PlayAudioAsync(audioClient, filePath,cancellationToken);
            sessionData.CurrentSongIndex++;
            
            if (sessionData.CurrentSongIndex >= sessionData.SongDatas.Count) {
                sessionData.CurrentSongIndex = 0;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally {
            sessionData.IsPlaying = false;
            Console.WriteLine($"Playing = {sessionData.IsPlaying}");
            if (sessionData.CurrentSongIndex < sessionData.SongDatas.Count) {
                await PlayNext(audioClient);
            }
            else {
                await Task.Delay(10000);
                songDataService.GetGuildSessionData(guildId).ActiveSession = false;
                await audioClient.StopAsync();
            }
        }
    }

    private async Task PlayAudioAsync(IAudioClient? audioClient, string filePath,CancellationToken cancellationToken) {
        logger.LogInformation($"started playing: {filePath}");
        try {
            var ffmpeg = Process.Start(new ProcessStartInfo {
                FileName = "ffmpeg",
                Arguments = $"-loglevel error -i \"{filePath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            
            if (ffmpeg == null) {
                Console.WriteLine("Failed to start ffmpeg process.");
                return;
            }

            await using var output = ffmpeg.StandardOutput.BaseStream;
            await using var discord = audioClient.CreatePCMStream(AudioApplication.Mixed);

            try {
                await output.CopyToAsync(discord,cancellationToken);
            }
            catch (OperationCanceledException) {
                logger.LogInformation("Audio playback was canceled.");
                await discord.FlushAsync(CancellationToken.None);
            }
            finally {
                await discord.FlushAsync(CancellationToken.None);
            }
            await ffmpeg.WaitForExitAsync(cancellationToken);
        }
        
        catch (Exception ex) {
            Console.WriteLine($"Error playing audio: {ex.Message}");
        }
    }

    private async Task<string> DownloadCheck(ulong guildId, string url, (string, string) fetch,
        CancellationToken cancellationToken) {
        try {
            var videoId = VideoIdFromUrl(url);
            logger.LogInformation($"VideoId: {videoId}");
            var (exist, filePath) = await guildDbEntry.SongExist(videoId);
            if (exist) {
                logger.LogInformation($"File {filePath} already exists");
                return filePath;
            }

            logger.LogInformation($"Downloading video {url}");
            var newFilePath = await YoutubeDownload.Download(url, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation($"filepath=  {newFilePath}");
            if (newFilePath == string.Empty) return newFilePath;
            var (id, title) = fetch;
            await guildDbEntry.SaveSong(id, title, newFilePath);
            return newFilePath;
        }
        catch (OperationCanceledException e) {
            Console.WriteLine(e);
            return "cancelled";
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    private static string VideoIdFromUrl(string videoUrl) {
        var uri = new Uri(videoUrl);
        var query = uri.Query;
        var queryParameters = HttpUtility.ParseQueryString(query);
        var videoId = queryParameters["v"];

        return videoId;
    }

    [SlashCommand("saveplaylist", "Save Playlist")]
    public async Task SavePlaylist(string name) {
        var guildId = Context.Guild.Id;
        var songDatas = songDataService.GetGuildSessionData(guildId).SongDatas;
        if (songDatas.Count > 0) {
            Console.WriteLine($"Saving playlist: {name}");
            await guildDbEntry.SavePlaylist(guildId, name, songDataService.GetGuildSessionData(guildId).SongDatas);
            await RespondAsync($"Playlist saved: {name}");
            return;
        }
        await RespondAsync("No current playlist found.");
    }
    
    [SlashCommand("skipdeneme", "Skip Deneme")]
    public async Task Skip() {
        var guildId = Context.Guild.Id;
        var sessionData = songDataService.GetGuildSessionData(guildId);

        if (!sessionData.ActiveSession || sessionData.SongDatas.Count == 0) {
            await RespondAsync("No song is currently playing to skip.", ephemeral: true);
            return;
        }
        await sessionData.PlaybackCancellationTokenSource?.CancelAsync()!;
        sessionData.PlaybackCancellationTokenSource = null;
        
        sessionData.CurrentSongIndex++;
        if (sessionData.CurrentSongIndex >= sessionData.SongDatas.Count) {
            sessionData.CurrentSongIndex = 0; // Loop back if at the end of the queue
        }
        
        await PlayNext(sessionData.AudioClient);
        await RespondAsync("Skipped to the next song!");
    }

    [SlashCommand("resetdeneme","reset deneme")]
    public async Task Reset() {
        var guildId = Context.Guild.Id;
        await songDataService.ResetData(guildId);
        await RespondAsync("Reset deneme");
        
    }
}
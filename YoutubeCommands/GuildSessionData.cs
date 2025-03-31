using System.Collections.Concurrent;
using Discord.Audio;

namespace BzBotDiscordNet.YoutubeCommands;

public class GuildSessionData
{
    public ConcurrentQueue<string> SongUrlsToDownload = new ConcurrentQueue<string>();
    public readonly List<SongData> SongDatas = [];
    public int CurrentSongIndex = 0;
    public bool IsPlaying = false;
    public ulong CurrentChannelId = 0;
    public bool IsConnected = false;
    public IAudioClient? AudioClient = null;
    public bool IsDownloading = false;
    public bool ActiveSession = false;
    public CancellationTokenSource? PlaybackCancellationTokenSource { get; set; }
    public CancellationTokenSource? DownloadCancellationTokenSource { get; set; }

    public List<string>? GetQueue()
    {
        var titles= SongDatas.Select(song => song.Title).ToList();
        for (int i = 0; i < titles.Count; i++)
        {
            titles[i] = titles[i]+=$" {SongDatas[i].Url}";
        }
        return titles;
    }
}
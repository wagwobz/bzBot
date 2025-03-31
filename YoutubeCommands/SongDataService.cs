using Microsoft.Extensions.Logging;

namespace BzBotDiscordNet.YoutubeCommands;

//Holds data for guild until bot reset, to save data indefinetely use database or json files.
public class SongDataService(ILogger<SongDataService> logger)
{
    private readonly Dictionary<ulong, GuildSessionData> _songListByGuildId = new Dictionary<ulong, GuildSessionData>();

    public GuildSessionData GetGuildSessionData(ulong guildId) {
        if (!GuildHasQueue(guildId)) {
            _songListByGuildId.Add(guildId, new GuildSessionData());
        }

        return _songListByGuildId.GetValueOrDefault(guildId);
    }

    private bool GuildHasQueue(ulong guildId) {
        return _songListByGuildId.ContainsKey(guildId);
    }

    public void AddPlaylistToDownloadList(List<string> urlList, ulong guildId) {
        if (!GuildHasQueue(guildId)) {
            _songListByGuildId.Add(guildId, new GuildSessionData());
        }

        foreach (var url in urlList) {
            AddUrlToDownloadList(url, guildId);
        }
    }

    public void AddUrlToDownloadList(string url, ulong guildId) {
        if (!GuildHasQueue(guildId)) {
            _songListByGuildId.Add(guildId, new GuildSessionData());
        }

        if (!_songListByGuildId[guildId].SongUrlsToDownload.Contains(url)) {
            _songListByGuildId[guildId].SongUrlsToDownload.Enqueue(url);
        }

        logger.LogInformation($"Added: {url}");
    }

    public bool ContinueDownload(ulong guildId) {
        return _songListByGuildId[guildId].SongUrlsToDownload.Count > 0;
    }

    public (bool, string) ShouldDownload(ulong guildId, string urlId) {
        var songData = _songListByGuildId[guildId].SongDatas.FirstOrDefault(x => x.urlId == urlId);
        return songData != null ? (true, Id: songData.urlId) : (false, string.Empty);
    }

    public (bool success, string url) DequeueUrlFromDownloadListToDownload(ulong guildId) {
        var success = _songListByGuildId[guildId].SongUrlsToDownload.TryDequeue(out var url);

        return (success, url);
    }

    public async Task ResetData(ulong guildId) {
        var sessionData = GetGuildSessionData(guildId);
        if (sessionData.DownloadCancellationTokenSource != null)
            await sessionData.DownloadCancellationTokenSource.CancelAsync();
        if (sessionData.PlaybackCancellationTokenSource != null)
            await sessionData.PlaybackCancellationTokenSource.CancelAsync();
        if (sessionData.AudioClient != null) await sessionData.AudioClient.StopAsync()!;
        _songListByGuildId[guildId] = new GuildSessionData();
    }
}
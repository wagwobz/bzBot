using Discord.Rest;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace bzbotDiscordNet.DownloadSystem;

public static class YoutubeDownload
{
    public static async Task<string> Download(string url, CancellationToken cancellationToken) {
        var path = string.Empty;
        var iteration = 0;
        
        var ytdl = new YoutubeDL();
        try {
            while (path == string.Empty && iteration < OptionList.Options.Count) {
                var loadOptions = OptionList.Options[iteration];
                Console.WriteLine(loadOptions.ToString());
                var result = await ytdl.RunAudioDownload(url, overrideOptions: loadOptions,ct: cancellationToken);
                path = result.Data;
                Console.WriteLine($"path: {path}");
                if (path == string.Empty) {
                    iteration++;
                }
            }
            return path;
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return path;
        }
    }

    public static async Task<(string id, string title)> FetchVideoData(string url) {
        var id = string.Empty;
        var title = string.Empty;
        try {
            var ytdl = new YoutubeDL();
            var options1 = new OptionSet() {
                NoContinue = true,
                NoRestrictFilenames = true,
                Format = "wa",
                CookiesFromBrowser = "firefox",
                ExtractorArgs = "youtube:player_client=default,-web_creator",
            };
            var fetch = await ytdl.RunVideoDataFetch(url);
            id = fetch.Data.ID;
            title = fetch.Data.Title;
            return (id, title);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return (id, title);
        }
    }
    public static async Task<string> DownloadById(string id, CancellationToken cancellationToken) {
        return await Download(YoutubeUrlFromId(id), cancellationToken);
    }

    private static string YoutubeUrlFromId(string id) {
        return $"https://www.youtube.com/watch?v={id}";
    }
}
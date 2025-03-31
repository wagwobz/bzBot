using System.Web;

namespace BzBotDiscordNet.YoutubeCommands;

public static class Extensions
{
    public static bool IsPlaylist(string url)
    {
        var uri = new Uri(url);
        var query = uri.Query;
        var queryParameters = HttpUtility.ParseQueryString(query);
        var hasPlaylist = queryParameters["list"] != null;
        return hasPlaylist;
    }

    public static bool IsVideo(string url)
    {
        var uri = new Uri(url);
        var query = uri.Query;
        var queryParameters = HttpUtility.ParseQueryString(query);
        var hasVideo = queryParameters["v"] != null;
        return hasVideo;
    }

    public static bool IsValidUrl(string url)
    {
        try
        {
            // Parse the URL
            var uri = new Uri(url);

            // Check if the host matches YouTube's domains
            var isYoutubeUrl = uri.Host.Equals("www.youtube.com", StringComparison.OrdinalIgnoreCase) ||
                               uri.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                               uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);
            return isYoutubeUrl && (IsVideo(url) || IsPlaylist(url));
        }
        catch
        {
            // If the URL cannot be parsed, it's not a valid YouTube URL
            return false;
        }
    }
}
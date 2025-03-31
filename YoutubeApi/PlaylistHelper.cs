using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace bzbotDiscordNet.YoutubeApi;

//YoutubeDLSharp download all the playlist, to play one by one we use youtubeapi to get individual urls from the playlist and download one by one using YoutubeDLSharp. 
public static class PlaylistHelper
{
    private const string BaseUrl = "https://youtube.googleapis.com/youtube/v3/playlistItems?";
    private const int MaxResults = 50;

    private const string BaseVideoUrl = "https://www.youtube.com/watch?v=";

    public static async Task<List<string>> VideoUrlListFromPlaylist(string playlistUrl, string apiKey)
    {
        var playlistId = PlayListIdFromUrl(playlistUrl);
        var nptExist = false;
        JsonElement nextPageTokenJsonElement = default;
        var videoIdElements = new List<JsonElement>();
        var baseString = $"{BaseUrl}part=contentDetails&maxResults={MaxResults}&playlistId={playlistId}&key={apiKey}";

        try
        {
            using (var client = new HttpClient())
            {
                // Console.WriteLine("Using HttpClient");
                do
                {
                    // Console.WriteLine($"base: {baseString}");
                    if (nptExist)
                    {
                        baseString += $"&pageToken={nextPageTokenJsonElement.GetString()}";
                    }

                    // Console.WriteLine($"get: {baseString}");
                    var response = await client.GetAsync(baseString);
                    var s = response.EnsureSuccessStatusCode().ToString();
                    Console.WriteLine(s);
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(content);
                    var root = jsonDocument.RootElement;
                    nptExist = root.TryGetProperty("nextPageToken", out nextPageTokenJsonElement);
                    // Console.WriteLine($"next page: {nptExist}");
                    videoIdElements.AddRange(root.GetProperty("items").EnumerateArray()
                        .Select(videoId => videoId.GetProperty("contentDetails").GetProperty("videoId")));
                } while (nptExist);
            }

            List<string> videoIds = videoIdElements.Select(videoIdElement => videoIdElement.GetString()).ToList();
            // Console.WriteLine(string.Join(", ", videoIds));
            // await File.WriteAllTextAsync(FilePath, content);
            for (int i = 0; i < videoIds.Count; i++)
            {
                var id = videoIds[i];
                videoIds[i] = $"{BaseVideoUrl}{id}";
            }

            return videoIds;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static string PlayListIdFromUrl(string playlistUrl)
    {
        var uri = new Uri(playlistUrl);

        // Get the query string
        var query = uri.Query;

        // Parse the query string into a collection
        var queryParameters = HttpUtility.ParseQueryString(query);

        // Extract the 'list' parameter
        var playlistId = queryParameters["list"];

        // Output the playlist ID
        Console.WriteLine($"Playlist ID: {playlistId}");

        return playlistId;
    }
}
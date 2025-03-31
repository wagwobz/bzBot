namespace BzBotDiscordNet.YoutubeCommands;

public class SongData(string urlId, string title, string filePath, string url)
{
    public string Title { get; init; } = title;
    public string urlId { get; init; } = urlId;
    public string FilePath { get; init; } = filePath;

    public string Url { get; init; } = url;
}
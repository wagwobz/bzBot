using YoutubeDLSharp.Options;

namespace bzbotDiscordNet.DownloadSystem;

public static class OptionList
{
    public static List<OptionSet> Options = [
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "ba",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "251",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "250",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "249",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "ba",
            ExtractorArgs = "youtube:player_client=ios,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "ba",
            ExtractorArgs = "youtube:player_client=tv,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "ba*",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "600",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "599",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "wa",
            CookiesFromBrowser = "firefox",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "wa",
            ExtractorArgs = "youtube:player_client=ios,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "wa",
            ExtractorArgs = "youtube:player_client=tv,-web_creator"
        },
        new OptionSet {
            Output = @"Downloads\%(id)s.%(ext)s",
            NoContinue = true,
            Format = "wa*",
            ExtractorArgs = "youtube:player_client=default,-web_creator"
        },
    ];
}
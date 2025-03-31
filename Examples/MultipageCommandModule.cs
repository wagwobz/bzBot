using BzBotDiscordNet.YoutubeCommands;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BzBotDiscordNet;

public class MultipageCommandModule(IServiceProvider serviceProvider,
    SongDataService songDataService,
    ILogger<SoundCommandModule> logger): InteractionModuleBase<SocketInteractionContext>
{
    private readonly List<string> _pages = new List<string>
    {
        "Page 1: Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! " +
        "Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation! Welcome to the multi-page navigation!",
        "Page 2: This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content." +
        " This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content. This is an example of multi-page content.",
        "Page 3: You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below." +
        " You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below. You can navigate using the buttons below.",
        "Page 4: This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing!" +
        " This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing! This is the last page. Thanks for viewing!"
    };

    [SlashCommand("multipage", "Show a multi-page message with buttons")]
    public async Task ShowPagesAsync()
    {
        var currentPageIndex = 0;

        // Create navigation buttons
        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == _pages.Count - 1)
            .Build();

        var embed = QueueEmbed(currentPageIndex, _pages.Count);
        await RespondAsync(embed: embed, components: components);
    }

    [ComponentInteraction("page_prev:*")]
    public async Task HandlePreviousPage(string index)
    {
        int currentPageIndex = int.Parse(index);
        currentPageIndex = Math.Max(0, currentPageIndex - 1);

        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == _pages.Count - 1)
            .Build();
        await Context.Interaction.DeferAsync();
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = QueueEmbed(currentPageIndex, _pages.Count);
            msg.Components = components;
        });
    }
    
    
    
    [ComponentInteraction("page_next:*")]
    public async Task HandleNextPage(string index)
    {
        Console.WriteLine($"Handling 'Next' button for index: {index}");
        int currentPageIndex = int.Parse(index);
        currentPageIndex = Math.Min(_pages.Count - 1, currentPageIndex + 1);

        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == _pages.Count - 1)
            .Build();
        await Context.Interaction.DeferAsync();
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = QueueEmbed(currentPageIndex, _pages.Count);
            msg.Components = components;
        });
    }

    public Embed QueueEmbed(int page,int totalPages)
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Music Queue")
            .WithDescription($"Page {page+1}/{totalPages}");

        //name -> title, object -> link
        embedBuilder.AddField("Line 1:",_pages[page],inline:true);
        embedBuilder.AddField("Line 2:",_pages[page],inline:true);
        embedBuilder.AddField("Line 3:",_pages[page],inline:true);
        embedBuilder.AddField("Field with a Link", "Here is a [link](https://example.com) in the field text.",
            inline: false);
        
        var embed = embedBuilder.Build();
        return embed;
    }
    
    
}
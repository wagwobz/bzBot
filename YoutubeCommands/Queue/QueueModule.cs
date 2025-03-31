using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace BzBotDiscordNet.YoutubeCommands.Queue;

public class QueueModule(SongDataService songDataService): InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("queue", "Play queue")]
    public async Task QueueAsync()
    {
        var guildId = Context.Guild.Id;
        var songCount = songDataService.GetGuildSessionData(guildId).SongDatas.Count;
        var totalPages = songCount / 10;
        if(songCount % 10 != 0) totalPages++; 
        Console.WriteLine(totalPages);
        var currentPageIndex = 0;
        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == totalPages - 1)
            .Build();

        var embed = QueueEmbed(currentPageIndex, totalPages);
        await RespondAsync(embed: embed, components: components);
    }
    
    [ComponentInteraction("page_prev:*")]
    public async Task HandlePreviousPage(string index)
    {
        Console.WriteLine($"Handling 'Previous' button for index: {index}");
        var guildId = Context.Guild.Id;
        var songCount = songDataService.GetGuildSessionData(guildId).SongDatas.Count;
        var totalPages = songCount / 10;
        if(songCount % 10 != 0) totalPages++; 
        Console.WriteLine(totalPages);
        int currentPageIndex = int.Parse(index);
        currentPageIndex = Math.Max(0, currentPageIndex - 1);

        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == totalPages - 1)
            .Build();
        await Context.Interaction.DeferAsync();
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = QueueEmbed(currentPageIndex, totalPages);
            msg.Components = components;
        });
    }
    
    
    
    [ComponentInteraction("page_next:*")]
    public async Task HandleNextPage(string index)
    {
        Console.WriteLine($"Handling 'Next' button for index: {index}");
        var guildId = Context.Guild.Id;
        var songCount = songDataService.GetGuildSessionData(guildId).SongDatas.Count;
        var totalPages = songCount / 10;
        if(songCount % 10 != 0) totalPages++; 
        Console.WriteLine(totalPages);
        int currentPageIndex = int.Parse(index);
        currentPageIndex = Math.Min(totalPages - 1, currentPageIndex + 1);

        var components = new ComponentBuilder()
            .WithButton("Previous", $"page_prev:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == 0)
            .WithButton("Next", $"page_next:{currentPageIndex}", ButtonStyle.Primary, disabled: currentPageIndex == totalPages - 1)
            .Build();
        await Context.Interaction.DeferAsync();
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = QueueEmbed(currentPageIndex, totalPages);
            msg.Components = components;
        });
    }
    
    public Embed QueueEmbed(int page,int totalPages)
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Music Queue")
            .WithDescription($"Page {page+1}/{totalPages}");
        
        var guildId = Context.Guild.Id;
        var songList = songDataService.GetGuildSessionData(guildId).SongDatas;
        var pageData = songList.Skip(page * 10).Take(10).ToList();

        for (int i = 0; i < pageData.Count; i++)
        {
            embedBuilder.AddField("\u200B", $"{page*10+i+1}. [{pageData[i].Title}]({pageData[i].Url})",
                inline: false);
        }
        
        var embed = embedBuilder.Build();
        return embed;
    }
}
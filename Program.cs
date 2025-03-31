using BzBotDiscordNet;
using BzBotDiscordNet.GuildDB;
using BzBotDiscordNet.GuildDB.Models;
using BzBotDiscordNet.YoutubeCommands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

var serviceProvider = CreateServices();

var client = serviceProvider!.GetRequiredService<DiscordSocketClient>();
var interactionHandler = serviceProvider.GetRequiredService<InteractionHandler>();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();

var token = configuration.GetValue<string>("DISCORD_TOKEN");

var db = serviceProvider.GetRequiredService<GuildDbContext>();
await db.Database.EnsureCreatedAsync();
await db.SaveChangesAsync();

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await interactionHandler.InitializeAsync();

client.Ready += () =>
{
    Console.WriteLine("Bot is connected!");
    return Task.CompletedTask;
};

client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;


//TO DO: control isPlaying(name it soundSessionActive) in here for a guild 
async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
{
    if (user.Id == client.CurrentUser.Id)
    {
        if (before.VoiceChannel == null && after.VoiceChannel != null)
        {
            Console.WriteLine($"Bot joined {after.VoiceChannel.Name}");
        }
        else if (before.VoiceChannel != null && after.VoiceChannel == null)
        {
            Console.WriteLine($"Bot left {before.VoiceChannel.Name}");
            var service = serviceProvider.GetRequiredService<SongDataService>();
            await service.ResetData(before.VoiceChannel.Guild.Id);
        }
        else if (before.VoiceChannel != null && after.VoiceChannel != null)
        {
            Console.WriteLine($"Bot moved {after.VoiceChannel.Name} from {before.VoiceChannel.Name}");
        }
    }
}

await Task.Delay(-1);
return;


static IServiceProvider CreateServices()
{
    var discordSocketConfig = new DiscordSocketConfig()
    {
        MessageCacheSize = 100,
        GatewayIntents = GatewayIntents.AllUnprivileged,
        ConnectionTimeout = int.MaxValue,
    };
    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    var collection = new ServiceCollection()
        .AddLogging(logging => logging.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Trace))
        .AddSingleton(discordSocketConfig)
        .AddSingleton<IConfiguration>(configuration)
        .AddDbContext<GuildDbContext>()
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<SongDataService>()
        .AddSingleton<GuildDbEntry>()
        .AddSingleton(x =>
        {
            var discordClient = x.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discordClient, new InteractionServiceConfig
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async,
                LogLevel = LogSeverity.Info
            });
        })
        .AddSingleton<InteractionHandler>();


    return collection.BuildServiceProvider();
}
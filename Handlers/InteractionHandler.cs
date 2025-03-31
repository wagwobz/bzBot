using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


//Slash command handler with DI
namespace BzBotDiscordNet;

internal class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactionService,
    IServiceProvider serviceProvider,
    ILogger<InteractionHandler> loggingService,
    IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public async Task InitializeAsync()
    {
        // Add modules from the current assembly
        await using var scope = serviceProvider.CreateAsyncScope();
        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);

        // Hook up events
        client.InteractionCreated += HandleInteractionAsync;
        interactionService.InteractionExecuted += OnInteractionExecuted;

        //Register commands to specific guilds. You can use global command registeration, however it does not update it immediately. For testing use guild registration.
        client.Ready += async () =>
        {
            // Console.WriteLine("Registering global commands...");
            // await _interactionService.RegisterCommandsGloballyAsync();
            // Console.WriteLine("Global commands registered.");
            
            var guildId = configuration.GetSection("GuildIDs").GetValue<ulong>("guildID1");; // Replace with your guild ID
            var guildId2 = configuration.GetSection("GuildIDs").GetValue<ulong>("guildID2"); // Replace with your guild ID
            await RegisterGuildCommandsAsync(guildId);
            await RegisterGuildCommandsAsync(guildId2);
        };
    }
    
    
    
    private async Task RegisterGuildCommandsAsync(ulong guildId)
    {
        try
        {
            // Register commands for the specific guild
            await interactionService.RegisterCommandsToGuildAsync(guildId);
            Console.WriteLine($"Guild-specific commands registered for guild ID: {guildId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering guild-specific commands: {ex.Message}");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            var result = await interactionService.ExecuteCommandAsync(context, serviceProvider);
            if (interaction is SocketMessageComponent component)
            {
                Console.WriteLine($"CustomId received: {component.Data.CustomId}");
            }
            if (!result.IsSuccess)
            {
                loggingService.LogError(result.ErrorReason, $"Error executing command: {result.Error}");
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.RespondAsync($"Error: {result.ErrorReason}", ephemeral: true);
                }
            }
            else
            {
                loggingService.LogTrace(message: $"Successfully created interaction for {interaction.User.GlobalName}.");
            }
        }
        catch (Exception ex)
        {
            loggingService.LogError(ex, $"Exception in HandleInteractionAsync {ex.Message}");
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.RespondAsync("An error occurred while processing this command.", ephemeral: true);
            }
        }
    }
    
    private async Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (string.IsNullOrEmpty(result.ErrorReason))
        {
            Console.WriteLine($"Successfully executed interaction for {context.User.Mention}.");
            return;
        }
        loggingService.LogError($"Interaction error for {context.Interaction}.");
        await context.Interaction.HandleWithResultAsync(result);
    }
    
    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            await interactionService.ExecuteCommandAsync(context, serviceProvider);
        }
        catch (Exception exception)
        {
            loggingService.LogError(exception, "Exception occurred whilst attempting to handle interaction.");
        }
    }
}
public static class DiscordInteractionExtensions
{
    /// <summary>
    /// Handles the interaction by sending a response or follow-up message based on the <paramref name="result"/>.
    /// </summary>
    /// <param name="interaction">The interaction to handle.</param>
    /// <param name="result">
    /// The result of an operation, which determines the style and description of the embed message.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task HandleWithResultAsync(this IDiscordInteraction interaction, IResult result)
    {
        var embed = new EmbedBuilder()
            .WithStyle(result.IsSuccess ? new SuccessfulEmbedStyle() : new UnsuccessfulEmbedStyle())
            .WithDescription(result.ErrorReason)
            .Build();

        if (interaction.HasResponded)
            await interaction.FollowupAsync(embed: embed).ConfigureAwait(false);
        else
            await interaction.RespondAsync(embed: embed).ConfigureAwait(false);
    }
}

public class SuccessfulEmbedStyle : EmbedStyle
{
    /// <inheritdoc/>
    public override string Name => "Succeed!";

    /// <inheritdoc/>
    public override string IconUrl => Icons.Check;

    /// <inheritdoc/>
    public override Color Color => Colors.Success;
}

public static class Colors
{
    /// <summary>
    /// The color used to indicate an informative state.
    /// </summary>
    public static readonly Color Primary = new(59, 163, 232);

    /// <summary>
    /// The color used to depict an emotion of positivity.
    /// </summary>
    public static readonly Color Success = new(43, 182, 115);

    /// <summary>
    /// The color used to depict an emotion of negativity.
    /// </summary>
    public static readonly Color Danger = new(231, 76, 60);
}

public static class Icons
{
    /// <summary>
    /// The icon used to indicate a success state.
    /// </summary>
    public const string Check = "https://cdn.discordapp.com/emojis/1199976868057718876.webp?size=96&quality=lossless";

    /// <summary>
    /// The icon used to indicate an error state.
    /// </summary>
    public const string Cross = "https://cdn.discordapp.com/emojis/1199976870410715196.webp?size=96&quality=lossless";
}

public static class EmbedBuilderExtensions
{
    /// <summary>
    /// Applies an <see cref="EmbedStyle"/> for the current embed builder.
    /// </summary>
    /// <param name="builder">The current builder.</param>
    /// <param name="style">An <see cref="EmbedStyle"/> to apply.</param>
    /// <returns>The current builder instance with the style applied.</returns>
    public static EmbedBuilder WithStyle(this EmbedBuilder builder, EmbedStyle style)
        => builder.WithAuthor(style.Name, style.IconUrl).WithColor(style.Color);
}

public abstract class EmbedStyle
{
    /// <summary>
    /// Gets or sets the name of the embed style.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets or sets the icon URL of the embed style.
    /// </summary>
    public abstract string IconUrl { get; }

    /// <summary>
    /// Gets or sets the color of the embed style.
    /// </summary>
    public abstract Color Color { get; }
}

public class UnsuccessfulEmbedStyle : EmbedStyle
{
    /// <inheritdoc/>
    public override string Name => "Woops!";

    /// <inheritdoc/>
    public override string IconUrl => Icons.Cross;

    /// <inheritdoc/>
    public override Color Color => Colors.Danger;
}

using System.ComponentModel.DataAnnotations;

namespace BzBotDiscordNet.GuildDB.Models;

public class DbGuild
{
    [MaxLength(18)]
    public ulong Id { get; set; }

    [MaxLength(30)]
    public string Name { get; set; } = null!;
}
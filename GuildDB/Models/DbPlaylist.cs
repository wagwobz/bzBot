using System.ComponentModel.DataAnnotations;

namespace BzBotDiscordNet.GuildDB.Models;

public class DbPlaylist
{
    public int Id { get; set; }

    [MaxLength(18)]
    public ulong GuildId { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    public List<DbSong> Songs { get; init; } = [];
}
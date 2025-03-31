using System.ComponentModel.DataAnnotations;

namespace BzBotDiscordNet.GuildDB.Models;

public class DbSong
{
    [MaxLength(11)]
    public string UrlId { get; set; } = null!;
    [MaxLength(100)]
    public string Title { get; set; } = null!;
    
    [MaxLength(200)]
    public string FilePath { get; set; } = null!;
}
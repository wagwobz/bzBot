using System.Xml.Schema;
using BzBotDiscordNet.GuildDB.Models;
using BzBotDiscordNet.YoutubeCommands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BzBotDiscordNet.GuildDB;

public class GuildDbEntry(GuildDbContext context, DiscordSocketClient client, ILogger<GuildDbEntry> logger)
{
    //song download controlled by SoundCommandModule
    public async Task SavePlaylist(ulong guildId, string name, List<SongData> songs) {
        try {
//guild control
            logger.LogDebug($"Saving playlist: {name}");
            if (!context.DbGuilds.Any(x => x.Id == guildId)) {
                var guildName = client.GetGuild(guildId).Name;
                context.DbGuilds.Add(new DbGuild {
                    Id = guildId,
                    Name = guildName,
                });
            }


            //song control
            var dbSongs = new List<DbSong>();
            foreach (var songData in songs) {
                var existingSong = await context.DbSongs.FindAsync(songData.urlId);
                if (existingSong != null) {
                    dbSongs.Add(existingSong);
                }


                else if (existingSong == null) {
                    var newSong = new DbSong {
                        UrlId = songData.urlId,
                        Title = songData.Title,
                        FilePath = songData.FilePath,
                    };
                    await context.DbSongs.AddAsync(newSong);
                    dbSongs.Add(newSong);
                }
            }

            //add playlist
            await context.DbPlaylists.AddAsync(new DbPlaylist {
                Name = name,
                GuildId = guildId,
                Songs = dbSongs
            });
            await context.SaveChangesAsync();
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<(bool, string)> SongExist(string songId) {
        try {
            var exist = await context.DbSongs.AnyAsync(x => x.UrlId == songId);
            var song = await context.DbSongs.FirstOrDefaultAsync(x => x.UrlId == songId);
            var str = exist ? $"Song exist: {song?.Title}" : "No such song";
            logger.LogDebug(str);
            return exist ? (exist, song.FilePath) : (exist, string.Empty);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SaveSong(string id, string title, string newFilePath) {
        try {
            var exist = await context.DbSongs.AnyAsync(x => x.UrlId == id);
            if (!exist) {
                await context.DbSongs.AddAsync(new DbSong {
                    UrlId = id,
                    Title = title,
                    FilePath = newFilePath,
                });
                await context.SaveChangesAsync();
            }
            else {
                logger.LogDebug($"Already Exist: {id}");
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
}
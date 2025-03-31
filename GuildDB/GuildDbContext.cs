using BzBotDiscordNet.GuildDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BzBotDiscordNet.GuildDB;

public class GuildDbContext : DbContext
{
    IConfiguration _configuration;
    ILogger _logger;
    
    public GuildDbContext(DbContextOptions options, IConfiguration configuration, ILogger logger)
        : base(options) {
        _configuration = configuration;
        _logger = logger;
    }

    public GuildDbContext(IConfiguration configuration, ILogger<GuildDbContext> logger, ILogger logger1) {
        _configuration = configuration;
        _logger = logger;
    }

    public DbSet<DbGuild> DbGuilds { get; set; } = null!;
    public DbSet<DbPlaylist> DbPlaylists { get; set; } = null!;
    public DbSet<DbSong> DbSongs { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder ) {
        optionsBuilder
            .UseSqlite(_configuration.GetConnectionString("SqlLiteConnectionString"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbGuild>()
            .HasMany<DbPlaylist>()
            .WithOne()
            .HasForeignKey(p => p.GuildId)
            .IsRequired();;

        modelBuilder.Entity<DbPlaylist>()
            .HasMany(e => e.Songs)
            .WithMany();

        modelBuilder.Entity<DbSong>()
            .HasKey(s => s.UrlId);
    }
}
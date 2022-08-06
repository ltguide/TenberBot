#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TenberBot.Features.ExperienceFeature.Data.Models;

namespace TenberBot.Features.ExperienceFeature.Data;

public class DataContext : DbContext
{
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment hostEnvironment;

    public DataContext(
        DbContextOptions<DataContext> options,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment) : base(options)
    {
        this.configuration = configuration;
        this.hostEnvironment = hostEnvironment;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder == null)
            return;

        if (hostEnvironment.IsDevelopment())
            optionsBuilder.EnableSensitiveDataLogging();

        optionsBuilder.UseSqlServer(
            configuration["app-database"],
            options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServerUser>()
            .HasKey(x => new { x.GuildId, x.UserId });
    }

    public DbSet<ServerUser> ServerUsers { get; set; }

    public DbSet<UserVoiceChannel> UserVoiceChannels { get; set; }

    public DbSet<UserLevel> UserLevels { get; set; }

    public DbSet<RankCard> RankCards { get; set; }
}
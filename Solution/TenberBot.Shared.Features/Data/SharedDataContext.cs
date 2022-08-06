#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TenberBot.Shared.Features.Data.Models;

namespace TenberBot.Shared.Features.Data;

public class SharedDataContext : DbContext
{
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment hostEnvironment;

    public SharedDataContext(
        DbContextOptions<SharedDataContext> options,
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

    public DbSet<ServerSetting> ServerSettings { get; set; }

    public DbSet<ChannelSetting> ChannelSettings { get; set; }

    public DbSet<UserStat> UserStats { get; set; }

    public DbSet<InteractionParent> InteractionParents { get; set; }

    public DbSet<Visual> Visuals { get; set; }
}
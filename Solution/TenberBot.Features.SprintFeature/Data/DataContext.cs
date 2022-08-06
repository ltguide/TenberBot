#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TenberBot.Features.SprintFeature.Data.Models;

namespace TenberBot.Features.SprintFeature.Data;

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

    public DbSet<SprintSnippet> SprintSnippets { get; set; }

    public DbSet<Sprint> Sprints { get; set; }

    public DbSet<UserSprint> UserSprints { get; set; }
}
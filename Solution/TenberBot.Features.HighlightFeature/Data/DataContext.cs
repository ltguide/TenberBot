#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TenberBot.Features.HighlightFeature.Data.Models;

namespace TenberBot.Features.HighlightFeature.Data;

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

    public DbSet<HighFive> HighFives { get; set; }
}
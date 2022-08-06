#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TenberBot.Features.PatFeature.Data.Models;

namespace TenberBot.Features.PatFeature.Data;

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

    public DbSet<Pat> Pats { get; set; }
}
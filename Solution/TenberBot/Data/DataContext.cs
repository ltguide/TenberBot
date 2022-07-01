#nullable disable

using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder?.EnableSensitiveDataLogging();
#endif
    }

    public DbSet<GlobalSetting> GlobalSettings { get; set; }

    public DbSet<BotStatus> BotStatuses { get; set; }

    public DbSet<Greeting> Greetings { get; set; }
}
#nullable disable

using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Console.WriteLine("DataContext hola");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder?.EnableSensitiveDataLogging();
#endif
    }

    public DbSet<GlobalSetting> Settings { get; set; }

    public DbSet<BotStatus> BotStatuses { get; set; }
}
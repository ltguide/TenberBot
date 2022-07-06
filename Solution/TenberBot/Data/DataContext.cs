﻿#nullable disable

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

    public DbSet<UserLevel> UserLevels { get; set; }

    public DbSet<UserStat> UserStats { get; set; }

    public DbSet<InteractionParent> InteractionParents { get; set; }

    public DbSet<BotStatus> BotStatuses { get; set; }

    public DbSet<Visual> Visuals { get; set; }

    public DbSet<Greeting> Greetings { get; set; }

    public DbSet<Hug> Hugs { get; set; }

    public DbSet<SprintSnippet> SprintSnippets { get; set; }

    public DbSet<SprintChannel> SprintChannels { get; set; }

    public DbSet<Sprint> Sprints { get; set; }

    public DbSet<UserSprint> UserSprints { get; set; }
}
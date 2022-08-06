using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.ExperienceFeature.Data;
using TenberBot.Features.ExperienceFeature.Data.Services;
using TenberBot.Features.ExperienceFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.ExperienceFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddSingleton<GuildExperienceService>();
        services.AddHostedService(provider => provider.GetRequiredService<GuildExperienceService>());
        services.AddSingleton<IGuildMessageService>(provider => provider.GetRequiredService<GuildExperienceService>());

        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IRankCardDataService, RankCardDataService>();
        services.AddTransient<IUserVoiceChannelDataService, UserVoiceChannelDataService>();
        services.AddTransient<IUserLevelDataService, UserLevelDataService>();
    }
}

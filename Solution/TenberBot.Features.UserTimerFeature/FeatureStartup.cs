using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.UserTimerFeature.Data;
using TenberBot.Features.UserTimerFeature.Data.Services;
using TenberBot.Features.UserTimerFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.UserTimerFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddSingleton<UserTimerService>();
        services.AddHostedService(provider => provider.GetRequiredService<UserTimerService>());

        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IUserTimerDataService, UserTimerDataService>();
    }
}

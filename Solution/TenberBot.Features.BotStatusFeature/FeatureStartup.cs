using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.BotStatusFeature.Data;
using TenberBot.Features.BotStatusFeature.Data.Services;
using TenberBot.Features.BotStatusFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.BotStatusFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddHostedService<BotStatusService>();

        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IBotStatusDataService, BotStatusDataService>();
    }
}

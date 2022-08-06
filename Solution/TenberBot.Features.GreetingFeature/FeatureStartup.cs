using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.GreetingFeature.Data;
using TenberBot.Features.GreetingFeature.Data.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.GreetingFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IGreetingDataService, GreetingDataService>();
    }
}

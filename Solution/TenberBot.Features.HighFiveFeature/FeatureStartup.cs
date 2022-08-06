using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.HighFiveFeature.Data;
using TenberBot.Features.HighFiveFeature.Data.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.HighFiveFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IHighFiveDataService, HighFiveDataService>();
    }
}

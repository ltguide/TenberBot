using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.FortuneFeature.Data;
using TenberBot.Features.FortuneFeature.Data.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.FortuneFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IFortuneDataService, FortuneDataService>();
    }
}

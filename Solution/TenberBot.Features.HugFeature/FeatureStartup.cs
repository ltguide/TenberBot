using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.HugFeature.Data;
using TenberBot.Features.HugFeature.Data.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.HugFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IHugDataService, HugDataService>();
    }
}

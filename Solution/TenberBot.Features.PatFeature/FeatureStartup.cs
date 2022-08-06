using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.PatFeature.Data;
using TenberBot.Features.PatFeature.Data.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.PatFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IPatDataService, PatDataService>();
    }
}

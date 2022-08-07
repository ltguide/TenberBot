using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.HighlightFeature.Data;
using TenberBot.Features.HighlightFeature.Data.Services;
using TenberBot.Features.HighlightFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.HighlightFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        //services.AddSingleton<HighlightService>();
        //services.AddSingleton<IGuildMessageService>(provider => provider.GetRequiredService<HighlightService>());

        //services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        //services.AddTransient<IHighlightDataService, HighlightDataService>();
    }
}

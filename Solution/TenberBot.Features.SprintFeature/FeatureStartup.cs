using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.SprintFeature.Data;
using TenberBot.Features.SprintFeature.Data.Services;
using TenberBot.Features.SprintFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.SprintFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddSingleton<SprintService>();
        services.AddHostedService(provider => provider.GetRequiredService<SprintService>());

        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<ISprintDataService, SprintDataService>();
        services.AddTransient<ISprintSnippetDataService, SprintSnippetDataService>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.HelpFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.HelpFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddTransient<IHelpService, HelpService>();
    }
}

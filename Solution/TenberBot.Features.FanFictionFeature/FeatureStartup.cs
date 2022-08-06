using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.FanFictionFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.FanFictionFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddHttpClient<StoryWebService>();
    }
}

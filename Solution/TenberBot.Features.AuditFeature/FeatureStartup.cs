using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.AuditFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.AuditFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddHostedService<AuditService>();
    }
}

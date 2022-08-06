using Microsoft.Extensions.DependencyInjection;

namespace TenberBot.Shared.Features;

public interface IFeatureStartup
{
    void AddFeature(IServiceCollection services);
}

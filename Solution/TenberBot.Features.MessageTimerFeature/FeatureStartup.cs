using Microsoft.Extensions.DependencyInjection;
using TenberBot.Features.MessageTimerFeature.Data;
using TenberBot.Features.MessageTimerFeature.Data.Services;
using TenberBot.Features.MessageTimerFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.MessageTimerFeature;

[FeatureStartup]
public class FeatureStartup : IFeatureStartup
{
    public void AddFeature(IServiceCollection services)
    {
        services.AddSingleton<MessageTimerService>();
        services.AddHostedService(provider => provider.GetRequiredService<MessageTimerService>());

        services.AddDbContext<DataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IMessageTimerDataService, MessageTimerDataService>();
    }
}

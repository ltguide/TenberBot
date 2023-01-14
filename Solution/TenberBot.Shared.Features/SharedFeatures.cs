using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Attributes.Settings;
using TenberBot.Shared.Features.Attributes.UserStats;
using TenberBot.Shared.Features.Attributes.Visuals;
using TenberBot.Shared.Features.Converters;
using TenberBot.Shared.Features.Data;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Shared.Features;

[FeatureStartup]
public class SharedFeatures : IFeatureStartup
{
    public static readonly List<Assembly> Assemblies = new();
    public static readonly Dictionary<Type, string> ServerSettings = new();
    public static readonly Dictionary<Type, string> ChannelSettings = new();
    public static readonly List<string> Visuals = new();
    public static readonly List<string> UserStats = new();

    public static readonly DateTime BaseDuration = new(9999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new IEmoteJsonConverter(), }
    };

    public void AddFeature(IServiceCollection services)
    {
        services.AddSingleton<CacheService>();
        services.AddHostedService(provider => provider.GetRequiredService<CacheService>());

        services.AddDbContext<SharedDataContext>(ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddTransient<IServerSettingDataService, ServerSettingDataService>();
        services.AddTransient<IChannelSettingDataService, ChannelSettingDataService>();

        services.AddTransient<IVisualDataService, VisualDataService>();
        services.AddTransient<IInteractionParentDataService, InteractionParentDataService>();

        services.AddTransient<IUserStatDataService, UserStatDataService>();

        services.AddHttpClient<VisualWebService>();
    }

    public static void RegisterFeatures(IServiceCollection services, string path)
    {
        RegisterFeature(services, Assembly.GetExecutingAssembly());

        foreach (var fileName in Directory.EnumerateFiles(path, "TenberBot.Features.*.dll", SearchOption.AllDirectories))
            RegisterFeature(services, fileName);
    }

    public static void RegisterFeature(IServiceCollection services, string fileName)
    {
        try
        {
            RegisterFeature(services, Assembly.LoadFrom(fileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RegisterFeature ({fileName}): {ex}");
        }
    }

    public static void RegisterFeature(IServiceCollection services, Assembly assembly)
    {
        if (Assemblies.Contains(assembly))
            return;

        try
        {
            var types = assembly.GetTypes();

            var startupType = types.FirstOrDefault(x => x.GetCustomAttribute<FeatureStartupAttribute>() != null);
            if (startupType == null)
            {
                Console.WriteLine($"no FeatureServiceAttribute detected: {assembly.FullName}");
                return;
            }

            Assemblies.Add(assembly);

            if (typeof(IFeatureStartup).IsAssignableFrom(startupType))
                (Activator.CreateInstance(startupType) as IFeatureStartup)?.AddFeature(services);

            foreach (var x in GetTypesByAttribute<ServerSettingsAttribute>(types))
                ServerSettings.Add(x.Key, x.Value);

            foreach (var x in GetTypesByAttribute<ChannelSettingsAttribute>(types))
                ChannelSettings.Add(x.Key, x.Value);

            Visuals.AddRange(GetVisuals(types));

            UserStats.AddRange(GetUserStats(types));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RegisterFeature ({assembly.FullName}): {ex}");
        }
    }

    private static Dictionary<Type, string> GetTypesByAttribute<T>(IList<Type> types) where T : SettingsAttribute
    {
        return types.Where(x => x.GetCustomAttribute<T>() != null)
            .ToDictionary(x => x, x => x.GetCustomAttribute<T>()!.Key);
    }

    private static IEnumerable<string> GetVisuals(IList<Type> types)
    {
        return types.Where(x => x.GetCustomAttribute<VisualsAttribute>() != null)
            .SelectMany(x => x.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(string))
            .Select(x => (string)x.GetValue(null)!));
    }

    private static IEnumerable<string> GetUserStats(IList<Type> types)
    {
        return types.Where(x => x.GetCustomAttribute<UserStatsAttribute>() != null)
            .SelectMany(x => x.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(string))
            .Select(x => (string)x.GetValue(null)!));
    }
}

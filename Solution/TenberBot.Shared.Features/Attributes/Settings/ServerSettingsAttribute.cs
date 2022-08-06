namespace TenberBot.Shared.Features.Attributes.Settings;

[AttributeUsage(AttributeTargets.Class)]
public class ServerSettingsAttribute : SettingsAttribute
{
    public ServerSettingsAttribute(string key) : base(key)
    {
    }
}

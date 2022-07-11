namespace TenberBot.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ServerSettingsAttribute : SettingsAttribute
{
    public ServerSettingsAttribute(string key) : base(key)
    {
    }
}

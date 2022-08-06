namespace TenberBot.Shared.Features.Attributes.Settings;

[AttributeUsage(AttributeTargets.Class)]
public class ChannelSettingsAttribute : SettingsAttribute
{
    public ChannelSettingsAttribute(string key) : base(key)
    {
    }
}

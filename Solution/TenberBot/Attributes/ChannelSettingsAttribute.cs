namespace TenberBot.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ChannelSettingsAttribute : SettingsAttribute
{
    public ChannelSettingsAttribute(string key) : base(key)
    {
    }
}

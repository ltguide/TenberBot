namespace TenberBot.Shared.Features.Attributes.Settings;

public class SettingsAttribute : Attribute
{
    public string Key { get; }

    public SettingsAttribute(string key)
    {
        Key = key;
    }
}

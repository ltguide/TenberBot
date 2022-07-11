namespace TenberBot.Attributes;

public class SettingsAttribute : Attribute
{
    public string Key { get; }
    public SettingsAttribute(string key)
    {
        Key = key;
    }
}

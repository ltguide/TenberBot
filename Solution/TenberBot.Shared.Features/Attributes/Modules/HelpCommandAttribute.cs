namespace TenberBot.Shared.Features.Attributes.Modules;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class HelpCommandAttribute : Attribute
{
    public string Group { get; }

    public string Arguments { get; }

    public string Description { get; }

    public HelpCommandAttribute(string? arguments = null, string? description = null, string? group = null)
    {
        Arguments = arguments ?? "";
        Description = description ?? "";
        Group = group ?? "";
    }
}

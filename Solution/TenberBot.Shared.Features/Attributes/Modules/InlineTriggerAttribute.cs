using System.Text.RegularExpressions;

namespace TenberBot.Shared.Features.Attributes.Modules;

[AttributeUsage(AttributeTargets.Method)]
public class InlineTriggerAttribute : Attribute
{
    public Regex Regex { get; }

    public InlineTriggerAttribute(string pattern, RegexOptions options)
    {
        Regex = new Regex(pattern, options | RegexOptions.Compiled);
    }
}

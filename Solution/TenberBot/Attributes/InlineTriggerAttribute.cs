using System.Text.RegularExpressions;

namespace TenberBot.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class InlineTriggerAttribute : Attribute
{
    public Regex Regex { get; }

    public InlineTriggerAttribute(string pattern, RegexOptions options)
    {
        Regex = new Regex(pattern, options | RegexOptions.Compiled);
    }
}

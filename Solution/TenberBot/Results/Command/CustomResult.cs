using Discord.Commands;

namespace TenberBot.Results.Command;

public class CustomResult : RuntimeResult
{
    public CustomResult(CommandError? error, string reason) : base(error, reason)
    {
    }

    public static CustomResult FromError(string reason)
    {
        return new CustomResult(CommandError.Unsuccessful, reason);
    }

    public static CustomResult FromSuccess(string reason = null!)
    {
        return new CustomResult(null, reason);
    }
}
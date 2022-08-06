using Discord.Commands;

namespace TenberBot.Shared.Features.Results.Command;

public class RemainResult : RuntimeResult
{
    public RemainResult(CommandError? error, string reason) : base(error, reason)
    {
    }

    public static RemainResult FromError(string reason)
    {
        return new RemainResult(CommandError.Unsuccessful, reason);
    }

    public static RemainResult FromSuccess(string reason = null!)
    {
        return new RemainResult(null, reason);
    }
}
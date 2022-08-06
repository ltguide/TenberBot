using Discord.Commands;

namespace TenberBot.Shared.Features.Results.Command;

public class DeleteResult : RuntimeResult
{
    public DeleteResult(CommandError? error, string reason) : base(error, reason)
    {
    }

    public static DeleteResult FromError(string reason)
    {
        return new DeleteResult(CommandError.Unsuccessful, reason);
    }

    public static DeleteResult FromSuccess(string reason = null!)
    {
        return new DeleteResult(null, reason);
    }
}
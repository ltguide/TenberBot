using Discord.Interactions;

namespace TenberBot.Shared.Features.Results.Interaction;

public class EphemeralResult : RuntimeResult
{
    public EphemeralResult(InteractionCommandError? error, string reason) : base(error, reason)
    {
    }

    public static EphemeralResult FromError(string reason)
    {
        return new EphemeralResult(InteractionCommandError.Unsuccessful, reason);
    }

    public static EphemeralResult FromSuccess(string reason = null!)
    {
        return new EphemeralResult(null, reason);
    }
}

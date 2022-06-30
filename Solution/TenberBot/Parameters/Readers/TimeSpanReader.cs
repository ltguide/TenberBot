using Discord.Commands;
using TimeSpanParserUtil;

namespace TenberBot.Parameters;

public class TimeSpanReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        if (TimeSpanParser.TryParse(input ?? "", out var timeSpan) == false)
            timeSpan = TimeSpan.Zero;

        return Task.FromResult(TypeReaderResult.FromSuccess(timeSpan));
    }
}

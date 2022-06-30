using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

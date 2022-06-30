using Discord;
using Discord.Interactions;
using TimeSpanParserUtil;

namespace TenberBot.Parameters;

public class TimeSpanConverter : TypeConverter<TimeSpan>
{
    public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        if (TimeSpanParser.TryParse(option.Value as string ?? "", out var timeSpan) == false)
            timeSpan = TimeSpan.Zero;

        return Task.FromResult(TypeConverterResult.FromSuccess(timeSpan));
    }
}

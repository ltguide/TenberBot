using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TenberBot.Attributes;
using TenberBot.Results.Command;

namespace TenberBot.Modules.Command;

[Remarks("Convert")]
[Group("convert")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class ConvertCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<ConvertCommandModule> logger;

    public ConvertCommandModule(
        ILogger<ConvertCommandModule> logger)
    {
        this.logger = logger;
    }

    [Command("", ignoreExtraArgs: true)]
    [Priority(-1)]
    public Task<RuntimeResult> NoSubCommand()
    {
        return Task.FromResult<RuntimeResult>(RemainResult.FromError("Please provide a conversion of: `temperature`"));
    }

    [Command("temperature")]
    [Alias("temp")]
    [Summary("Convert temperatures.\nInclude the unit (C or F) to convert from.")]
    [Remarks("`<temp>`")]
    [InlineTrigger(@"\b(-?\d+(?:\.\d+)?)°? ?([CF])\b", RegexOptions.IgnoreCase)]
    public async Task<RuntimeResult> Temp([Remainder] string? word = null)
    {
        var match = Regex.Match(word ?? "", @"(-?\d+(?:\.\d+)?)°? ?([CF])", RegexOptions.IgnoreCase);
        if (match.Success == false)
            return DeleteResult.FromError("Please provide a temperature to convert, e.g. `86F` or `46.5° C`");

        var value = double.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToUpper();

        (double Value, string Unit) result;
        if (unit == "C")
            result = ((value * 1.8d) + 32, "F");
        else
            result = ((value - 32) * (5d / 9d), "C");

        await Context.Message.ReplyAsync($"{value:#,##0.##}° {unit} is {result.Value:#,##0.##}° {result.Unit}");

        return DeleteResult.FromSuccess();
    }
}

using Discord;
using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Results.Command;
using UnitsNet;
using UnitsNet.Units;

namespace TenberBot.Features.ConvertFeature.Modules.Command;

[Remarks("Convert")]
[Group("convert")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class ConvertCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IMemoryCache memoryCache;

    public ConvertCommandModule(
        IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

    [Command("", ignoreExtraArgs: true)]
    [Priority(-1)]
    public Task<RuntimeResult> Nothing()
    {
        return Task.FromResult<RuntimeResult>(RemainResult.FromError("Please provide a conversion of: `temperature`"));
    }

    [Command("temperature")]
    [Alias("temp")]
    [Summary("Convert temperatures.\nInclude the unit (C or F) to convert from.")]
    [Remarks("`<temp>`")]
    public async Task<RuntimeResult> Temp([Remainder] string? word = null)
    {
        var match = Regex.Match(word ?? "", @"(-?\d+(?:\.\d+)?)°? ?([CF])", RegexOptions.IgnoreCase);
        if (match.Success == false)
            return DeleteResult.FromError("Please provide a temperature to convert, e.g. `86F` or `46.5°C`");

        var value = double.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value;

        var (from, to) = ConvertTemperatures(value, unit);

        await Context.Message.ReplyAsync($"{from} is {to}");

        return DeleteResult.FromSuccess();
    }

    [Command("temperature-inline")]
    [InlineTrigger(@"(?:^|[^\w.])(-?\d+(?:\.\d+)?)°? ?([CF])(?=\W*(?:\s|$)|$)", RegexOptions.IgnoreCase)]
    public async Task TempInline(double value, string unit)
    {
        var (from, to) = ConvertTemperatures(value, unit);

        var key = $"temperature-inline, {Context.Channel.Id}, {(from.Unit == TemperatureUnit.DegreeCelsius ? from : to)}";

        if (memoryCache.TryGetValue<Temperature>(key, out var _))
            return;

        memoryCache.Set(key, from, TimeSpan.FromMinutes(30));

        await Context.Message.ReplyAsync($"{from} is {to}");
    }

    private static (Temperature From, Temperature To) ConvertTemperatures(double value, string unit)
    {
        if (unit.ToUpper() == "C")
        {
            var from = Temperature.FromDegreesCelsius(value);
            return (from, from.ToUnit(TemperatureUnit.DegreeFahrenheit));
        }
        else
        {
            var from = Temperature.FromDegreesFahrenheit(value);
            return (from, from.ToUnit(TemperatureUnit.DegreeCelsius));
        }
    }
}

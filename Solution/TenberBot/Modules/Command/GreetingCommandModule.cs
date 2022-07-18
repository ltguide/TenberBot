using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[Remarks("Greetings")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class GreetingCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex Variables = new(@"%random%|%user%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IVisualDataService visualDataService;
    private readonly IGreetingDataService greetingDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly ILogger<GreetingCommandModule> logger;

    public GreetingCommandModule(
        IVisualDataService visualDataService,
        IGreetingDataService greetingDataService,
        IUserStatDataService userStatDataService,

        ILogger<GreetingCommandModule> logger)
    {
        this.visualDataService = visualDataService;
        this.greetingDataService = greetingDataService;
        this.userStatDataService = userStatDataService;
        this.logger = logger;
    }

    [Command("just-bot-name")]
    public async Task Blank()
    {
        await SendRandom(GreetingType.BotName, VisualType.BotName);
    }

    [Command("hi", ignoreExtraArgs: true)]
    [Alias("hello", "hey")]
    [Summary("Say a Hello greeting.")]
    public async Task Hello()
    {
        await SendRandom(GreetingType.Hello, VisualType.Hello);
    }

    [Command("gb", ignoreExtraArgs: true)]
    [Alias("goodbye", "bye", "cya")]
    [Summary("Say a Good Bye greeting.")]
    public async Task Bye()
    {
        await SendRandom(GreetingType.Bye);
    }

    [Command("gm", ignoreExtraArgs: true)]
    [Alias("goodmorning", "morning")]
    [Summary("Say a Good Morning greeting.")]
    public async Task Morning()
    {
        await SendRandom(GreetingType.Morning);
    }

    [Command("ga", ignoreExtraArgs: true)]
    [Alias("goodafternoon", "afternoon")]
    [Summary("Say a Good Afternoon greeting.")]
    public async Task Afternoon()
    {
        await SendRandom(GreetingType.Afternoon);
    }

    [Command("ge", ignoreExtraArgs: true)]
    [Alias("goodevening", "evening")]
    [Summary("Say a Good Evening greeting.")]
    public async Task Evening()
    {
        await SendRandom(GreetingType.Evening);
    }


    [Command("gd", ignoreExtraArgs: true)]
    [Alias("goodday", "day")]
    [Summary("Say a Good Day greeting.")]
    public async Task Day()
    {
        await SendRandom(GreetingType.Day);
    }

    [Command("gn", ignoreExtraArgs: true)]
    [Alias("goodnight", "night")]
    [Summary("Say a Good Night greeting.")]
    public async Task Night()
    {
        await SendRandom(GreetingType.Night);
    }

    private async Task AddStat()
    {
        (await userStatDataService.GetOrAddByContext(Context)).Greetings++;

        await userStatDataService.Save();
    }

    private async Task<bool> SendRandom(GreetingType greetingType)
    {
        var greeting = await greetingDataService.GetRandom(greetingType);
        if (greeting == null)
            return false;

        var message = Variables.Replace(greeting.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%random%" => Context.GetRandomUser()?.GetDisplayNameSanitized() ?? "Random User",
                "%user%" => Context.User.GetDisplayNameSanitized(),
                _ => match.Value,
            };
        });

        await ReplyAsync(message);

        await AddStat();

        return true;
    }

    private async Task<bool> SendRandom(VisualType visualType)
    {
        var visual = await visualDataService.GetRandom(visualType);
        if (visual == null)
            return false;

        await Context.Channel.SendFileAsync(visual.AsAttachment());

        await AddStat();

        return true;
    }

    private async Task SendRandom(GreetingType greetingType, VisualType visualType)
    {
        if (Random.Shared.Next(2) == 0)
        {
            if (await SendRandom(visualType) == false)
                await SendRandom(greetingType);
        }
        else
        {
            if (await SendRandom(greetingType) == false)
                await SendRandom(visualType);
        }
    }
}

using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[RequireUserPermission(ChannelPermission.SendMessages)]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class GreetingCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex Variables = new(@"%random%|%user%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IGreetingDataService greetingDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly ILogger<GreetingCommandModule> logger;

    public GreetingCommandModule(
        IGreetingDataService greetingDataService,
        IUserStatDataService userStatDataService,
        ILogger<GreetingCommandModule> logger)
    {
        this.greetingDataService = greetingDataService;
        this.userStatDataService = userStatDataService;
        this.logger = logger;
    }

    [Command("hi")]
    [Alias("hello", "sup", "hola", "hey", "test")]
    [Summary("Hi there!")]
    public async Task Generic()
    {
        await SendRandomText(GreetingType.Generic);
    }

    [Command("gb")]
    [Alias("goodbye", "bye", "cya")]
    [Summary("Good Bye :)")]
    public async Task Bye()
    {
        await SendRandomText(GreetingType.Bye);
    }

    [Command("gm")]
    [Alias("goodmorning", "morning")]
    [Summary("Good Morning :)")]
    public async Task Morning()
    {
        await SendRandomText(GreetingType.Morning);
    }

    [Command("ga")]
    [Alias("goodafternoon", "afternoon")]
    [Summary("Good Afternoon :)")]
    public async Task Afternoon()
    {
        await SendRandomText(GreetingType.Afternoon);
    }

    [Command("ge")]
    [Alias("goodevening", "evening")]
    [Summary("Good Evening :)")]
    public async Task Evening()
    {
        await SendRandomText(GreetingType.Evening);
    }

    [Command("gn")]
    [Alias("goodnight", "night")]
    [Summary("Good Night :)")]
    public async Task Night()
    {
        await SendRandomText(GreetingType.Night);
    }

    private async Task SendRandomText(GreetingType greetingType)
    {
        var greeting = await greetingDataService.GetRandom(greetingType);
        if (greeting == null)
            return;

        (await userStatDataService.GetOrAddById(Context)).Greetings++;

        await userStatDataService.Save();

        var message = Variables.Replace(greeting.Text, (match) =>
        {
            return match.Value switch
            {
                "%random%" => Context.GetRandomUser()?.GetDisplayNameSanitized() ?? "Random User",
                "%user%" => Context.User.GetDisplayNameSanitized(),
                _ => match.Value,
            };
        });

        await ReplyAsync(message);
    }
}

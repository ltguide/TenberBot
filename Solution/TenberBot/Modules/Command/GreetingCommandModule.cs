using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[RequireUserPermission(ChannelPermission.SendMessages)]
public class GreetingCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IGreetingDataService greetingDataService;
    private readonly ILogger<GreetingCommandModule> logger;

    public GreetingCommandModule(
        IGreetingDataService greetingDataService,
        ILogger<GreetingCommandModule> logger)
    {
        this.greetingDataService = greetingDataService;
        this.logger = logger;
    }

    [Command("hi")]
    [Alias("hello", "sup", "hola", "hey", "test")]
    [Summary("Hi there!")]
    public async Task Generic()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Generic);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    [Command("gb")]
    [Alias("goodbye", "bye", "cya")]
    [Summary("Good Bye :)")]
    public async Task Bye()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Bye);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    [Command("gm")]
    [Alias("goodmorning", "morning")]
    [Summary("Good Morning :)")]
    public async Task Morning()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Morning);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    [Command("ga")]
    [Alias("goodafternoon", "afternoon")]
    [Summary("Good Afternoon :)")]
    public async Task Afternoon()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Afternoon);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    [Command("ge")]
    [Alias("goodevening", "evening")]
    [Summary("Good Evening :)")]
    public async Task Evening()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Evening);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    [Command("gn")]
    [Alias("goodnight", "night")]
    [Summary("Good Night :)")]
    public async Task Night()
    {
        var greeting = await greetingDataService.GetRandom(GreetingType.Night);
        if (greeting == null)
            return;

        await ReplyAsync(FormatMessage(greeting.Text));
    }

    private string FormatMessage(string message)
    {
        if (message.Contains("%random%") && Context.Channel is SocketTextChannel channel)
        {
            var randomUser = channel.Users.Where(x => x.IsBot == false && x != Context.User).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (randomUser != null)
                message = message.Replace("%random%", randomUser.GetDisplayNameSanitized());
        }

        return message;
    }
}

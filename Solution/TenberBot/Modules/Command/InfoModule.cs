using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Extensions;

namespace TenberBot.Modules;

public class InfoModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<InfoModule> _logger;

    public InfoModule(
        ILogger<InfoModule> logger)
    {
        _logger = logger;
    }

    [Command("ping")]
    [Alias("pong", "hello")]
    public async Task PingAsync()
    {
        _logger.LogInformation("User {user} used the ping command!", Context.User.Username);
        await ReplyAsync("pong!");
    }

    [Command("log")]
    public Task TestLogs()
    {
        _logger.LogTrace("This is a trace log");
        _logger.LogDebug("This is a debug log");
        _logger.LogInformation("This is an information log");
        _logger.LogWarning("This is a warning log");
        _logger.LogError(new InvalidOperationException("Invalid Operation"), "This is a error log with exception");
        _logger.LogCritical(new InvalidOperationException("Invalid Operation"), "This is a critical load with exception");

        _logger.Log(GetLogLevel(LogSeverity.Error), "Error logged from a Discord LogSeverity.Error");
        _logger.Log(GetLogLevel(LogSeverity.Info), "Information logged from Discord LogSeverity.Info ");

        return Task.CompletedTask;
    }

    private static LogLevel GetLogLevel(LogSeverity severity)
        => (LogLevel)Math.Abs((int)severity - 5);

    // ~say hello world -> hello world
    [Command("say")]
    [Summary("Echoes a message.")]
    public async Task Say([Remainder][Summary("The text to echo")] string echo)
    {
        await ReplyAsync($"{Context.User.GetDisplayName()} or {Context.User.Mention} told me to say: {echo}", messageReference: Context.Message.GetReferenceTo());
    }

    [Command("purge")]
    [Summary("Purge messages from channel.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task Purge(int count, bool quiet = false)
    {
        if (Context.Channel is not SocketTextChannel channel)
            return;

        count = Math.Min(Math.Max(1, count), 100);

        await Context.Message.AddReactionAsync(Program.EmoteUnknown);

        var messages = await Context.Channel.GetMessagesAsync(limit: count + 1).FlattenAsync();

        var reply = await ReplyAsync($"Deleting {count} message{(count != 1 ? "s" : "")}.");

        await channel.DeleteMessagesAsync(messages);

        if (quiet)
            await reply.DeleteAsync();
        else
            await reply.AddReactionAsync(Program.EmoteSuccess);
    }


    [Command("sprint")]
    [Summary("Create a sprint that others can join! Get stuff done!")]
    public async Task Sprint(TimeSpan duration, [Remainder] string task)
    {
        // duration > 3 minutes?
        // did user start a sprint?
        // did user join a sprint?

        var embed = new EmbedBuilder
        {
            Title = task,
            Description = $"In one minute, a sprint will begin for {duration}.",
            Color = Color.Blue,
            Author = Context.User.GetEmbedAuthor("is starting a sprint!"),
        };

        var component = new ComponentBuilder()
            .WithButton("Join Sprint", "custom-id:idhere");




        await Context.Message.ReplyAsync(embed: embed.Build(), components: component.Build());
    }

    [Command("react")]
    public async Task AddReaction()
    {


        //var guild = Context.Client.GetGuild(409053859328294913);

        //var guildCommand = new SlashCommandBuilder();
        //// Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
        //guildCommand.WithName("first-command");
        //// Descriptions can have a max length of 100.
        //guildCommand.WithDescription("This is my first guild slash command!");

        ////await guild.CreateApplicationCommandAsync(guildCommand.Build());
        //await guild.DeleteApplicationCommandsAsync();

        await Context.Message.AddReactionAsync(Program.EmoteSuccess);

        //await Context.Message.ReplyAsync(e)
    }

    [Command("hola")]
    public async Task DropPic(int x)
    {
        switch (x)
        {
            case 0:
                await Context.Channel.SendFileAsync(@"d:\temp\young-people-waving-hand-illustrations-set_23-2148373635.jpg");
                break;
            case 1:
                await Context.Channel.SendFileAsync(@"D:\temp\mochi-peachcat-mochi.gif");
                break;

        }

        var emote = Emote.Parse("<:kappam:416385924189126656>");

        await Context.Message.AddReactionAsync(emote);
    }

    [Command("embed")]
    public async Task SendRichEmbed()
    {
        var embed = new EmbedBuilder
        {
            // Embed property can be set within object initializer
            Title = "Hello world!",
            Description = "I am a description set by initializer."
        };
        // Or with methods
        embed.AddField("Field title",
            "Field value. I also support [hyperlink markdown](https://example.com)!")
            .WithAuthor(Context.Client.CurrentUser)
            .WithFooter(footer => footer.Text = "I am a footer.")
            .WithColor(Color.Blue)
            .WithTitle("I overwrote \"Hello world!\"")
            .WithDescription("I am a description.")
            .WithUrl("https://example.com")
            .WithCurrentTimestamp();

        //Your embed needs to be built before it is able to be sent
        await ReplyAsync(embed: embed.Build());
    }
}

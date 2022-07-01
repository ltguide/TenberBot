using Discord;
using Discord.Commands;
using TenberBot.Data;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

public class InfoCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<InfoCommandModule> logger;

    public InfoCommandModule(
        ILogger<InfoCommandModule> logger)
    {
        this.logger = logger;
    }

    [Command("say")]
    [Summary("Echoes a message.")]
    public async Task Say([Remainder] string text)
    {
        await Context.Message.ReplyToAsync($"{Context.User.GetDisplayNameSanitized()} or {Context.User.Mention} told me to say: {text}");
    }

    [Command("react")]
    public async Task AddReaction()
    {
        await Context.Message.AddReactionsAsync(new[] { GlobalSettings.EmoteSuccess, GlobalSettings.EmoteFail, GlobalSettings.EmoteUnknown });
    }


    //[Command("sprint")]
    //[Summary("Create a sprint that others can join! Get stuff done!")]
    //public async Task Sprint(TimeSpan duration, [Remainder] string task)
    //{
    //    // duration > 3 minutes?
    //    // did user start a sprint?
    //    // did user join a sprint?

    //    var embed = new EmbedBuilder
    //    {
    //        Title = task,
    //        Description = $"In one minute, a sprint will begin for {duration}.",
    //        Color = Color.Blue,
    //        Author = Context.User.GetEmbedAuthor("is starting a sprint!"),
    //    };

    //    var component = new ComponentBuilder()
    //        .WithButton("Join Sprint", "custom-id:idhere");

    //    await Context.Message.ReplyAsync(embed: embed.Build(), components: component.Build());
    //}

    [Command("pics")]
    public async Task pics()
    {
        await Context.Channel.SendFileAsync(@"d:\temp\young-people-waving-hand-illustrations-set_23-2148373635.jpg");
        await Context.Channel.SendFileAsync(@"D:\temp\mochi-peachcat-mochi.gif");

        await Context.Message.AddReactionAsync(GlobalSettings.EmoteSuccess);
    }

    [Command("embed")]
    public async Task SendRichEmbed()
    {
        var embed = new EmbedBuilder
        {
            Title = "Hello world!",
            Description = "I am a description set by initializer."
        };

        embed.AddField("Field title",
            "Field value. I also support [hyperlink markdown](https://example.com)!")
            .WithAuthor(Context.Client.CurrentUser)
            .WithFooter(footer => footer.Text = "I am a footer.")
            .WithColor(Color.Blue)
            //.WithTitle("I overwrote \"Hello world!\"")
            //.WithDescription("I am a description.")
            .WithUrl("https://example.com")
            .WithCurrentTimestamp();

        //Your embed needs to be built before it is able to be sent
        await ReplyAsync(embed: embed.Build());
    }
}

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
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task Say([Remainder] string text)
    {
        await Context.Message.ReplyAsync($"{Context.User.GetDisplayNameSanitized()} told me to say: {text}");
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
}

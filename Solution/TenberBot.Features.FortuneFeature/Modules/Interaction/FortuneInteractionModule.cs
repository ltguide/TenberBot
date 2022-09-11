using Discord;
using Discord.Interactions;
using System.Text.RegularExpressions;
using TenberBot.Features.FortuneFeature.Data.Models;
using TenberBot.Features.FortuneFeature.Data.Services;
using TenberBot.Features.FortuneFeature.Data.Visuals;
using TenberBot.Features.FortuneFeature.Helpers;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordInteractions;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.FortuneFeature.Modules.Interaction;

[EnabledInDm(false)]
public class FortuneInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly static Regex SelfVariables = new(@"%user%|%random%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IFortuneDataService fortuneDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;

    public FortuneInteractionModule(
        IFortuneDataService fortuneDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService)
    {
        this.fortuneDataService = fortuneDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
    }

    [SlashCommand("fortune", "Seek insight from the oracle.")]
    [HelpCommand]
    public async Task Fortune(
        )
    {
        var visual = await visualDataService.GetRandom(Visuals.Fortune);
        if (visual == null)
            return;

        var fortune = await fortuneDataService.GetRandom();
        if (fortune == null)
            return;

        (await userStatDataService.GetOrAddByContext(Context)).Fortunes++;

        await userStatDataService.Save();

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("sought the oracle's insight!"),
            Color = Color.DarkGreen,
            ImageUrl = $"attachment://{visual.AttachmentFilename}",
        };

        using var memoryStream = FortuneImageHelper.GetStream(visual, GetMessage(fortune));

        await RespondWithFileAsync(new FileAttachment(memoryStream, visual.AttachmentFilename), embed: embedBuilder.Build());
    }

    private string GetMessage(Fortune fortune)
    {
        return SelfVariables.Replace(fortune.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetDisplayName(),
                "%random%" => Context.GetRandomUser()?.GetDisplayName() ?? "Random User",
                _ => match.Value,
            };
        });
    }
}

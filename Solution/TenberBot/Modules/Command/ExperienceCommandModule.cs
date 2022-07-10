using Discord;
using Discord.Commands;
using TenberBot.Data.Services;

namespace TenberBot.Modules.Command;

[Remarks("Experience")]
public class ExperienceCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IUserLevelDataService userLevelDataService;
    private readonly ILogger<ExperienceCommandModule> logger;

    public ExperienceCommandModule(
        IUserLevelDataService userLevelDataService,
        ILogger<ExperienceCommandModule> logger)
    {
        this.userLevelDataService = userLevelDataService;
        this.logger = logger;
    }

    [Command("level", ignoreExtraArgs: true)]
    [Summary("See your experience information.")]
    public async Task Level()
    {
        var userLevel = await userLevelDataService.GetByContext(Context);
        if (userLevel == null)
            return;

        await Context.Message.ReplyAsync($"TODO pretty message and rank 😊\nMessage level: {userLevel.MessageLevel} ({userLevel.MessageExperience:N2}/{userLevel.NextLevelMessageExperience:N0} exp)\nVoice level: {userLevel.VoiceLevel} ({userLevel.VoiceExperience:N2}/{userLevel.NextLevelVoiceExperience:N0} exp)");
    }
}

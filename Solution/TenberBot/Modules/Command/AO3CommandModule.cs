using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TenberBot.Attributes;
using TenberBot.Extensions;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

[Remarks("Story Info")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class AO3CommandModule : ModuleBase<SocketCommandContext>
{
    private readonly StoryWebService storyWebService;
    private readonly ILogger<AO3CommandModule> logger;

    public AO3CommandModule(
        StoryWebService storyWebService,
        ILogger<AO3CommandModule> logger)
    {
        this.storyWebService = storyWebService;
        this.logger = logger;
    }

    [Command("ao3")]
    [Summary("Fetch information on an AO3 work.")]
    [Remarks("`<url>`")]
    [InlineTrigger(@"\b(https://archiveofourown\.org/works/\d+)", RegexOptions.IgnoreCase)]
    public async Task<RuntimeResult> AO3([Remainder] string url)
    {
        var story = await storyWebService.GetAO3(url);
        if (story == null)
            return DeleteResult.FromError("I couldn't load this story 😭");

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("shared a story"),
            Color = story.GetRatingColor(),
            Description = $"***{story.Name}*** by {story.Author}\n",
        }
        .AddFieldChunkByComma("Fandom", story.Fandom)
        .AddFieldChunkByComma("Rating", story.Rating, true)
        .AddFieldChunkByComma("Warning", story.Warning, true)
        .AddFieldChunkByComma("Category", story.Category)
        .AddFieldChunkByComma("Tags", story.Tags)
        .AddFieldChunkByComma("Characters", story.Characters)
        .AddFieldChunkByComma("Relationships", story.Relationships)
        .AddFieldChunkByComma("Collections", story.Collections, true)
        .AddFieldChunkByComma("Series", story.Series, true)
        .AddFieldChunkByComma("Published", story.Published, true)
        .AddFieldChunkByComma(story.StatusName ?? "Status", story.Status, true)
        .AddFieldChunkByComma("Words", story.Words, true)
        .AddFieldChunkByComma("Chapters", story.Chapters, true)
        .AddFieldChunkByComma("Comments", story.Comments, true)
        .AddFieldChunkByComma("Kudos", story.Kudos, true)
        .AddFieldChunkByComma("Bookmarks", story.Bookmarks, true)
        .AddFieldChunkByComma("Hits", story.Hits, true)
        .AddFieldChunkByComma("Language", story.Language, true);

        if (story.Summary != null)
            embedBuilder.Description += $"\n**Summary**\n {story.Summary}\n";

        await Context.Message.ReplyAsync(embed: embedBuilder.Build());

        return DeleteResult.FromSuccess();
    }
}

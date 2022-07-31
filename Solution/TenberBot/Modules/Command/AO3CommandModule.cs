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
        };
        
        AddField(embedBuilder, "Fandom", story.Fandom);
        AddField(embedBuilder, "Rating", story.Rating, true);
        AddField(embedBuilder, "Warning", story.Warning, true);


        AddField(embedBuilder, "Category", story.Category);
        AddField(embedBuilder, "Tags", story.Tags);
        AddField(embedBuilder, "Characters", story.Characters);
        AddField(embedBuilder, "Relationships", story.Relationships);

        AddField(embedBuilder, "Collections", story.Collections, true);
        AddField(embedBuilder, "Series", story.Series, true);

        AddField(embedBuilder, "Published", story.Published, true);
        AddField(embedBuilder, story.StatusName ?? "Status", story.Status, true);
        AddField(embedBuilder, "Words", story.Words, true);
        AddField(embedBuilder, "Chapters", story.Chapters, true);
        AddField(embedBuilder, "Comments", story.Comments, true);
        AddField(embedBuilder, "Kudos", story.Kudos, true);
        AddField(embedBuilder, "Bookmarks", story.Bookmarks, true);
        AddField(embedBuilder, "Hits", story.Hits, true);

        AddField(embedBuilder, "Language", story.Language, true);

        if (story.Summary != null)
            embedBuilder.Description += $"\n**Summary**\n {story.Summary}\n";

        await Context.Message.ReplyAsync(embed: embedBuilder.Build());

        return DeleteResult.FromSuccess();
    }

    private static void AddField(EmbedBuilder embedBuilder, string name, string? value, bool inline = false)
    {
        if (value != null)
            embedBuilder.WithFields(value.ChunkByLines(1024).Select(x => new EmbedFieldBuilder { Name = name, Value = x.Replace(" \n", ", "), IsInline = inline, }));
    }
}

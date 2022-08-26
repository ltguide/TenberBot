using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using TenberBot.Features.FanFictionFeature.Services;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Extensions.DiscordEmbed;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.FanFictionFeature.Modules.Command;

[Remarks("Story Info")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class AO3CommandModule : ModuleBase<SocketCommandContext>
{
    private readonly StoryWebService storyWebService;

    public AO3CommandModule(
        StoryWebService storyWebService)
    {
        this.storyWebService = storyWebService;
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

        var embed = new EmbedBuilder
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
            embed.Description += $"\n**Summary**\n {story.Summary}\n";

        EmbedBuilder? extraEmbed = null;

        while (embed.Length > EmbedBuilder.MaxEmbedLength || embed.Fields.Count > EmbedBuilder.MaxFieldCount)
        {
            var index = embed.Fields.Count - 1;
            var field = embed.Fields[index];
            embed.Fields.RemoveAt(index);

            extraEmbed ??= new();

            extraEmbed.Fields.Insert(0, field);
        }

        await Context.Message.ReplyAsync(embed: embed.Build());

        if (extraEmbed != null)
            await ReplyAsync(embed: extraEmbed.Build());

        return DeleteResult.FromSuccess();
    }
}

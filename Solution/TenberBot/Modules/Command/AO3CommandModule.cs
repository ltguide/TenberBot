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
        var a = await storyWebService.GetAO3(url);
        if (a == null)
            return DeleteResult.FromError("I couldn't load this story 😭");

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("shared a story"),
            Color = Color.Gold,
            Description = $"***{a.Name}*** by {a.Author}\n",
        }
        .AddField("Fandom", a.Fandom)
        .AddField("Rating", a.Rating, true)
        .AddField("Warning", a.Warning, true);


        AddField(embedBuilder, "Category", a.Category);
        AddField(embedBuilder, "Tags", a.Tags);
        AddField(embedBuilder, "Characters", a.Characters);
        AddField(embedBuilder, "Relationships", a.Relationships);

        AddField(embedBuilder, "Collections", a.Collections, true);
        AddField(embedBuilder, "Series", a.Series, true);

        AddField(embedBuilder, "Published", a.Published, true);
        AddField(embedBuilder, a.StatusName ?? "Status", a.Status, true);
        AddField(embedBuilder, "Words", a.Words, true);
        AddField(embedBuilder, "Chapters", a.Chapters, true);
        AddField(embedBuilder, "Comments", a.Comments, true);
        AddField(embedBuilder, "Kudos", a.Kudos, true);
        AddField(embedBuilder, "Bookmarks", a.Bookmarks, true);
        AddField(embedBuilder, "Hits", a.Hits, true);

        AddField(embedBuilder, "Language", a.Language, true);

        if (a.Summary != null)
            embedBuilder.Description += $"\n**Summary**\n {a.Summary}\n";

        await Context.Message.ReplyAsync(embed: embedBuilder.Build());

        return DeleteResult.FromSuccess();
    }

    private static void AddField(EmbedBuilder embedBuilder, string name, string? value, bool inline = false)
    {
        if (value != null)
            embedBuilder.WithFields(value.ChunkByLines(1024).Select(x => new EmbedFieldBuilder { Name = name, Value = x.Replace("\n", ", "), IsInline = inline, }));
    }
}

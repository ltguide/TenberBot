using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Results.Command;
using TenberBot.Services;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages.Parsing;
using WikiClientLibrary.Sites;

namespace TenberBot.Modules.Command;

[RequireBotPermission(ChannelPermission.SendMessages)]
public class DefineCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly CacheService cacheService;
    private readonly ILogger<DefineCommandModule> logger;

    public DefineCommandModule(
        CacheService cacheService,
        ILogger<DefineCommandModule> logger)
    {
        this.cacheService = cacheService;
        this.logger = logger;
    }

    [Command("define")]
    [Alias("definition")]
    [Summary("What's that word?!")]
    [Remarks("`<word>`")]
    public async Task Define([Remainder] string word)
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        //https://en.wiktionary.org/wiki/Wiktionary:Entry_layout

        using var wikiClient = new WikiClient();

        var wikiSite = new WikiSite(wikiClient, "https://en.wiktionary.org/w/api.php");

        await wikiSite.Initialization;

        var doc = new HtmlDocument();
        try
        {
            var parsedContentInfo = await wikiSite.ParsePageAsync(word, ParsingOptions.ResolveRedirects | ParsingOptions.DisableToc | ParsingOptions.DisableEditSection);
            doc.LoadHtml(parsedContentInfo.Content);
        }
        catch (Exception)
        {
            var results = await wikiSite.OpenSearchAsync(word);
            var search = results.Count != 0 ? $"\nWiktionary suggested: {string.Join(", ", results.Select(x => x.Title))}" : " and Wiktionary had no suggestions for you.";

            await Context.Message.ReplyAsync($"Sorry, I couldn't find an exact match 😫{search}");
        }

        if (doc.DocumentNode.ChildNodes.Count > 0)
        {
            var embeds = ParseDefinition(doc).ChunkByLines(4096).Select((x, i) => new EmbedBuilder
            {
                Author = i == 0 ? Context.User.GetEmbedAuthor($"asked for the definition of {word}") : null,
                Color = Color.Teal,
                Description = x,
            }.Build()).ToArray();

            await Context.Message.ReplyAsync(embeds: embeds);
        }

        await Context.Message.RemoveAllReactionsForEmoteAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);
    }

    private static string ParseDefinition(HtmlDocument doc)
    {
        bool multipleEtymology = false;
        bool output = false;

        var excludedHeadings = new[] { "Alternative forms", "Glyph origin", "Pronunciation", "Production", "See also", "References", "Further reading", "Anagrams", };

        var sb = new StringBuilder();

        foreach (var node in doc.DocumentNode.FirstChild.ChildNodes)
        {
            if (node.NodeType != HtmlNodeType.Element)
                continue;

            if (node.Name == "hr")
                break;

            switch (node.Name)
            {
                case "h2":
                case "h5":
                case "h6":
                    output = false;
                    break;

                case "h3":
                    output = Array.IndexOf(excludedHeadings, node.InnerText) == -1;

                    if (node.InnerText.StartsWith("Etymology "))
                        multipleEtymology = true;
                    break;

                case "h4":
                    output = multipleEtymology && Array.IndexOf(excludedHeadings, node.InnerText) == -1;
                    break;
            }

            if (output)
            {
                if (node.Name == "ol")
                    OutputList(sb, node, 0);
                else
                    OutputNode(sb, node);
            }
        }

        return WebUtility.HtmlDecode(sb.ToString());
    }

    private static void OutputList(StringBuilder sb, HtmlNode node, int level)
    {
        var liNodes = node.SelectNodes("li[text()]|li[span]|li[a]");
        var levelX = string.Concat(Enumerable.Repeat("`  ` ", level));

        for (var i = 0; i < liNodes.Count; i++)
        {
            sb.Append(levelX);

            sb.Append($"`{i + 1,2}` ");

            foreach (var child in liNodes[i].SelectNodes("(*[not(self::ul)][not(self::dl)][not(self::ol)]|text())"))
            {
                var innerText = child.InnerText.TrimEnd('\n').SanitizeMD();

                if (child.HasClass("ib-content"))
                    sb.Append($"*{innerText}*");
                else
                    sb.Append(innerText);
            }

            sb.Append('\n');

            var olNodes = liNodes[i].SelectNodes("ol");
            if (olNodes != null)
                foreach (var ol in olNodes)
                    OutputList(sb, ol, level + 1);
        }
    }

    private static void OutputNode(StringBuilder sb, HtmlNode node)
    {
        var innerText = node.InnerText.TrimEnd('\n').SanitizeMD();

        switch (node.Name)
        {
            case "h3":
                if (innerText.StartsWith("Etymology"))
                    innerText = $"__{innerText}__";

                sb.Append($"\n**{innerText}**\n");
                break;

            case "h4":
                sb.Append($"\n**{innerText}**\n");
                break;

            case "style":
            case "div":
                break;

            default:
                if (innerText.Length > 0)
                    sb.Append($"> {innerText}\n");
                break;
        }
    }
}

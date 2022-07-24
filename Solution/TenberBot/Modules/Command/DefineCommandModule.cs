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
    public async Task<RuntimeResult> Define([Remainder] string word)
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        using var wikiClient = new WikiClient();

        var wikiSite = new WikiSite(wikiClient, "https://en.wiktionary.org/w/api.php");

        await wikiSite.Initialization;

        var doc = new HtmlDocument();
        try
        {
            var parsedContentInfo = await wikiSite.ParsePageAsync(word, ParsingOptions.ResolveRedirects | ParsingOptions.DisableToc | ParsingOptions.DisableEditSection);
            doc.LoadHtml(parsedContentInfo.Content);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"failed to load definition: {word}");

            return RemainResult.FromError("Sorry, I couldn't find a definition 😫 Wiktionary cares about capitalization, so perhaps you are looking for a Proper Noun?");
        }

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor($"asked for the definition of {word}"),
            Color = Color.Teal,
            Description = WebUtility.HtmlDecode(ParseDefinition(doc)),
        };

        await Context.Message.ReplyAsync(embed: embedBuilder.Build());

        await Context.Message.RemoveAllReactionsForEmoteAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        return RemainResult.FromSuccess();
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

            switch (node.Name)
            {
                case "hr":
                    return sb.ToString();

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
                OutputNode(sb, node);
        }

        return sb.ToString();
    }

    private static void OutputNode(StringBuilder sb, HtmlNode node)
    {
        var innerText = node.InnerText.TrimEnd('\n');

        switch (node.Name)
        {
            case "ol":
                var liNodes = node.SelectNodes("li[text()]|li[span]");

                for (var i = 0; i < liNodes.Count; i++)
                {
                    sb.Append($"`{i + 1,2}` ");

                    foreach (var child in liNodes[i].SelectNodes("(*[not(self::ul)][not(self::dl)][not(self::ol)]|text())"))
                    {
                        innerText = child.InnerText.TrimEnd('\n');

                        if (child.HasClass("ib-content"))
                            sb.Append($"*{innerText}*");
                        else
                            sb.Append(innerText);
                    }

                    sb.Append('\n');
                }

                return;

            case "h3":
                if (innerText.StartsWith("Etymology"))
                    innerText = $"__{innerText}__";

                sb.Append($"\n**{innerText}**\n");
                return;

            case "h4":
                sb.Append($"\n**{innerText}**\n");
                return;

            case "style":
            case "div":
                break;

            case "p":
            default:
                sb.Append($"> {innerText}\n");
                return;
        }
    }
}

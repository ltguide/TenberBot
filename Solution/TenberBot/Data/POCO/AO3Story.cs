﻿using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ReverseMarkdown;
using System.Net;

namespace TenberBot.Data.POCO;

public class AO3Story : Story
{
    private static readonly string BaseHref = "https://archiveofourown.org";
    public static bool TryParse(string url, string content, out Story story)
    {
        story = new AO3Story();

        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        if (doc.DocumentNode.ChildNodes.Count == 0)
            return false;

        var config = new Config
        {
            UnknownTags = Config.UnknownTagsOption.Drop,
            RemoveComments = true,
            SmartHrefHandling = true
        };

        var converter = new Converter(config);

        try
        {
            var preface = doc.GetElementbyId("workskin").QuerySelector("div.preface:not(.chapter)");

            story.Name = $"[{preface.SelectSingleNode("h2").InnerText.Trim()}]({url})";

            var author = preface.SelectSingleNode("h3/a");
            story.Author = $"[{author.InnerText}]({BaseHref}{author.GetAttributeValue("href", "")})";

            story.Summary = converter.Convert(preface.SelectSingleNode("//blockquote").InnerHtml);

            var dlMeta = doc.GetElementbyId("main").QuerySelector("dl.work.meta");

            foreach (var anchor in dlMeta.QuerySelectorAll("a[href^='/']"))
                anchor.SetAttributeValue("href", BaseHref + anchor.GetAttributeValue("href", ""));

            var meta = dlMeta.SelectNodes("dd").ToDictionary(x => x.GetClasses().First(), x => x);

            story.Rating = GetList(meta, "rating")!;
            story.Warning = GetList(meta, "warning")!;
            story.Fandom = GetList(meta, "fandom")!;
            story.Language = GetString(meta, "language")!;
            story.Category = GetList(meta, "category");
            story.Relationships = GetList(meta, "relationship");
            story.Characters = GetList(meta, "character");
            story.Tags = GetList(meta, "freeform");
            story.Series = meta.TryGetValue("series", out var node) ? WebUtility.HtmlDecode(node.QuerySelector("span.position").InnerText.Trim()) : null;
            story.Collections = GetString(meta, "collections");

            var stats = meta["stats"].SelectNodes("dl/dd").ToDictionary(x => x.GetClasses().First(), x => x);

            story.Published = GetString(stats, "published")!;
            story.Words = GetString(stats, "words")!;
            story.Chapters = GetString(stats, "chapters")!;
            story.Comments = GetString(stats, "comments");
            story.Kudos = GetString(stats, "kudos");
            story.Bookmarks = GetString(stats, "bookmarks");
            story.Hits = GetString(stats, "hits");

            story.Status = GetString(stats, "status");
            if (story.Status != null)
                story.StatusName = meta["stats"].QuerySelector("dt.status").InnerText.Trim();

            string? GetList(Dictionary<string, HtmlNode> dictionary, string key)
            {
                if (dictionary.TryGetValue(key, out var node))
                    return WebUtility.HtmlDecode(string.Join("\n", node.SelectNodes("ul/li").Select(x => converter.Convert(x.InnerHtml))));

                return null;
            }

            string? GetString(Dictionary<string, HtmlNode> dictionary, string key)
            {
                if (dictionary.TryGetValue(key, out var node))
                    return WebUtility.HtmlDecode(converter.Convert(node.InnerHtml));

                return null;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}

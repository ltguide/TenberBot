using Discord;
using Discord.Interactions;
using System.Text.RegularExpressions;
using TenberBot.Features.HighlightFeature.Data.Enums;
using TenberBot.Features.HighlightFeature.Data.Models;
using TenberBot.Features.HighlightFeature.Data.Services;
using TenberBot.Features.HighlightFeature.Modals.Ignore;
using TenberBot.Features.HighlightFeature.Modals.Word;
using TenberBot.Features.HighlightFeature.Services;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.POCO;
using TenberBot.Shared.Features.Extensions.Mentions;

namespace TenberBot.Features.HighlightFeature.Modules.Interaction;

[Group("highlight", "Manage highlight words and ignores.")]
[HelpCommand(group: "Highlight")]
[EnabledInDm(false)]
public class HighlightInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly static Regex ValidWord = new(@"^(\*?)(...+?)(\*?)$", RegexOptions.Compiled);

    private readonly IIgnoreUserDataService ignoreUserDataService;
    private readonly IIgnoreChannelDataService ignoreChannelDataService;
    private readonly IHighlightWordDataService highlightWordDataService;
    private readonly HighlightService highlightService;

    public HighlightInteractionModule(
        IIgnoreUserDataService ignoreUserDataService,
        IIgnoreChannelDataService ignoreChannelDataService,
        IHighlightWordDataService highlightWordDataService,
        HighlightService highlightService)
    {
        this.ignoreUserDataService = ignoreUserDataService;
        this.ignoreChannelDataService = ignoreChannelDataService;
        this.highlightWordDataService = highlightWordDataService;
        this.highlightService = highlightService;
    }

    [SlashCommand("ignore-channels", "Manage list of channels in which all messages are ignored. Optionally, provide a channel to add.")]
    [HelpCommand("`[channel]`")]
    public async Task IgnoreChannel(
        [Summary("add")][ChannelTypes(ChannelType.Voice, ChannelType.Text)] IChannel? channel = null)
    {
        if (channel == null)
        {
            await ListIgnores(IgnoreType.Channel);
            return;
        }

        var ignoreChannel = new IgnoreChannel
        {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            IgnoreChannelId = channel.Id,
        };

        if (await ignoreChannelDataService.Get(ignoreChannel) == null)
        {
            await ignoreChannelDataService.Add(ignoreChannel);
            highlightService.Add(ignoreChannel);
        }

        await RespondAsync("Gotcha! To see the current list, run the slash command again or use the refresh button.", ephemeral: true);
    }

    [SlashCommand("ignore-users", "Manage list of users of which all messages are ignored. Optionally, provide a user to add.")]
    [HelpCommand("`[user]`")]
    public async Task IgnoreUser(
        [Summary("add")] IUser? user = null)
    {
        if (user == null)
        {
            await ListIgnores(IgnoreType.User);
            return;
        }

        var ignoreUser = new IgnoreUser
        {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            IgnoreUserId = user.Id,
        };

        if (await ignoreUserDataService.Get(ignoreUser) == null)
        {
            await ignoreUserDataService.Add(ignoreUser);
            highlightService.Add(ignoreUser);
        }

        await RespondAsync("Gotcha! To see the current list, run the slash command again or use the refresh button.", ephemeral: true);
    }

    [ComponentInteraction("refresh-ignore:*")]
    public async Task RefreshIgnore(IgnoreType ignoreType)
    {
        var embed = await GetIgnoresEmbed(ignoreType);
        await DeferAsync();
        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }

    [ComponentInteraction("delete-ignore:*")]
    public async Task DeleteIgnore(IgnoreType ignoreType)
    {
        await Context.Interaction.RespondWithModalAsync<IgnoreDeleteModal>($"highlight delete-ignore:{ignoreType}", modifyModal: (builder) => builder.Title += ignoreType.ToString());
    }

    [ModalInteraction("delete-ignore:*")]
    public async Task DeleteIgnoreModal(IgnoreType ignoreType, IgnoreDeleteModal modal)
    {
        if (ignoreType == IgnoreType.Channel)
        {
            var ignoreChannel = await ignoreChannelDataService.GetByIndex(Context.Guild.Id, Context.User.Id, modal.Text);

            if (ignoreChannel == null)
            {
                await RespondAsync($"I couldn't find id #{modal.Text}.", ephemeral: true);
                return;
            }

            await ignoreChannelDataService.Delete(ignoreChannel);
            highlightService.Delete(ignoreChannel);
        }
        else
        {
            var ignoreUser = await ignoreUserDataService.GetByIndex(Context.Guild.Id, Context.User.Id, modal.Text);

            if (ignoreUser == null)
            {
                await RespondAsync($"I couldn't find id #{modal.Text}.", ephemeral: true);
                return;
            }

            await ignoreUserDataService.Delete(ignoreUser);
            highlightService.Delete(ignoreUser);
        }

        var embed = await GetIgnoresEmbed(ignoreType);
        await DeferAsync();
        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }

    [SlashCommand("words", "Manage list of words.")]
    [HelpCommand]
    public async Task Words()
    {
        await RespondAsync("You can use a `*` at the beginning and/or end to partially match words, e.g. `monkey`, `*key`, `mon*`, and `*nke*` will all match **monkey**. Any other `*` will be treated as a normal character.", ephemeral: true);

        await PageWords("", 0);
    }

    [ComponentInteraction("page-words:*,*")]
    public async Task PageWords(string _, int currentPage)
    {
        var view = new PageView { CurrentPage = currentPage, };
        view.PageCount = await highlightWordDataService.GetCount(Context.Guild.Id, Context.User.Id, view);

        if (Context.Interaction.HasResponded == false)
            await DeferAsync();

        var embed = await GetWordsEmbed(view);
        await ModifyOriginalResponseAsync(x =>
        {
            x.Embed = embed;
            x.Components = GetWordsComponents(view);
        });
    }

    [ComponentInteraction("add-word")]
    public async Task AddWord()
    {
        await Context.Interaction.RespondWithModalAsync<WordAddModal>($"highlight add-word");
    }

    [ModalInteraction("add-word")]
    public async Task AddWordModal(WordAddModal modal)
    {
        var match = ValidWord.Match(modal.Text);

        if (match.Success == false)
        {
            await RespondAsync("Do you like spam? Give me at least 3 normal characters to match.", ephemeral: true);
            return;
        }

        var highlightWord = new HighlightWord
        {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            Word = match.Groups[2].Value,
        };

        highlightWord.SetMatchLocation(match.Groups[1].Length != 0, match.Groups[3].Length != 0);

        if (await highlightWordDataService.Get(highlightWord) == null)
        {
            await highlightWordDataService.Add(highlightWord);
            highlightService.Add(highlightWord);
        }

        await PageWords("", 0);
    }


    [ComponentInteraction("delete-word")]
    public async Task DeleteWord()
    {
        await Context.Interaction.RespondWithModalAsync<WordDeleteModal>($"highlight delete-word");
    }

    [ModalInteraction("delete-word")]
    public async Task DeleteWordModal(WordDeleteModal modal)
    {
        var highlightWord = await highlightWordDataService.GetByIndex(Context.Guild.Id, Context.User.Id, modal.Text);

        if (highlightWord == null)
        {
            await RespondAsync($"I couldn't find id #{modal.Text}.", ephemeral: true);
            return;
        }

        await highlightWordDataService.Delete(highlightWord);
        highlightService.Delete(highlightWord);

        await PageWords("", 0);
    }

    private async Task ListIgnores(IgnoreType ignoreType)
    {
        var componentBuilder = new ComponentBuilder()
            .WithButton("Delete", $"highlight delete-ignore:{ignoreType}", ButtonStyle.Danger, new Emoji("🗑"))
            .WithButton(customId: $"highlight refresh-ignore:{ignoreType}", emote: new Emoji("🔁"));

        await RespondAsync($"To add a {ignoreType.ToString().ToLower()}, use the slash command and provide its name to the `add` parameter.", embed: await GetIgnoresEmbed(ignoreType), components: componentBuilder.Build(), ephemeral: true);
    }

    private async Task<Embed> GetIgnoresEmbed(IgnoreType ignoreType)
    {
        IEnumerable<string> lines;

        if (ignoreType == IgnoreType.Channel)
            lines = (await ignoreChannelDataService.GetAll(Context.Guild.Id, Context.User.Id)).Select((x, i) => $"`{i + 1,4}` {x.IgnoreChannelId.GetChannelMention()}");
        else
            lines = (await ignoreUserDataService.GetAll(Context.Guild.Id, Context.User.Id)).Select((x, i) => $"`{i + 1,4}` {x.IgnoreUserId.GetUserMention()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Ignored {ignoreType}s",
            Color = Color.Blue,
            Description = $"**`  Id` Name**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    private async Task<Embed> GetWordsEmbed(PageView view)
    {
        //Console.WriteLine($"{view.CurrentPage} of {view.PageCount + 1}");

        var words = await highlightWordDataService.GetPage(Context.Guild.Id, Context.User.Id, view);
        var lines = words.Select((x, i) => $"`{i + view.BaseIndex,4}` {x.GetText()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Highlight Words",
            Color = Color.Blue,
            Description = $"**`  Id` Word**\n{string.Join("\n", lines)}",
        };

        if (words.Count == 0)
            embedBuilder.Description += "\n*No results found.*";
        else
            embedBuilder.WithFooter($"Page {view.CurrentPage + 1} of {view.PageCount + 1}");

        return embedBuilder.Build();
    }

    private static MessageComponent GetWordsComponents(PageView view)
    {
        var componentBuilder = new ComponentBuilder()
            .WithButton(customId: "highlight page-words:first,0", emote: new Emoji("⏮"))
            .WithButton(customId: $"highlight page-words:previous,{Math.Max(0, view.CurrentPage - 1)}", emote: new Emoji("⏪"))
            .WithButton(customId: $"highlight page-words:next,{Math.Max(0, Math.Min(view.PageCount, view.CurrentPage + 1))}", emote: new Emoji("⏩"))
            .WithButton(customId: $"highlight page-words:last,{Math.Max(0, view.PageCount)}", emote: new Emoji("⏭"))
            .WithButton("Add", "highlight add-word", ButtonStyle.Success, new Emoji("➕"), row: 1)
            .WithButton("Delete", "highlight delete-word", ButtonStyle.Danger, new Emoji("🗑"), row: 1);

        return componentBuilder.Build();
    }
}

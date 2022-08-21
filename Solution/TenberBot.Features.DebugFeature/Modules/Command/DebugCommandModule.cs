using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Modules.Command;

[Remarks("Debug")]
public class DebugCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;

    public DebugCommandModule(
        DiscordSocketClient client,
        CacheService cacheService)
    {
        this.client = client;
        this.cacheService = cacheService;
    }

    [Command("debug-version", ignoreExtraArgs: true)]
    public async Task ShowVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            throw new InvalidOperationException();

        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";

        await Context.Message.ReplyAsync($"**{assembly.GetName().Name}** {version}\n\n*Modules:*\n> {string.Join("\n> ", SharedFeatures.Assemblies.Select(x => x.GetName().Name))}");
    }

    [Command("debug-roles", ignoreExtraArgs: true)]
    public async Task ShowRoles()
    {
        if (Context.User is not SocketGuildUser user)
            return;

        await Context.Message.ReplyAsync($"I think you have these roles: {string.Join(", ", user.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention))}", allowedMentions: AllowedMentions.None);
    }

    [Command("debug-latency", ignoreExtraArgs: true)]
    public async Task ShowLatency()
    {
        await Context.Message.ReplyAsync($"Most recent latency: {client.Latency}ms");
    }


    [Command("debug-avatar", ignoreExtraArgs: true)]
    public async Task ShowAvatars()
    {
        await Context.Message.ReplyAsync($"Out of the available, using this one: <{Context.User.GetCurrentAvatarUrl()}>\n> GetGuildAvatarUrl: {(Context.User as SocketGuildUser)?.GetGuildAvatarUrl()}\n> GetAvatarUrl: {Context.User.GetAvatarUrl()}\n> GetDefaultAvatarUrl: {Context.User.GetDefaultAvatarUrl()}");
    }

    [Command("debug-react", ignoreExtraArgs: true)]
    public async Task AddReaction()
    {
        var emotes = cacheService.Get<EmoteServerSettings>(Context.Guild);

        _ = Task.Run(async () =>
        {
            await Context.Message.AddReactionsAsync(new[] {
                emotes.Success,
                emotes.Fail,
                emotes.Busy,
            });
        });

        await ReplyAsync($"{emotes.Success.ToString()!.SanitizeMD()} / {emotes.Fail.ToString()!.SanitizeMD()} / {emotes.Busy.ToString()!.SanitizeMD()}");

        await ReplyAsync($"{emotes.Success} / {emotes.Fail} / {emotes.Busy}");
    }

    [Command("say")]
    [Summary("Echo a message.")]
    [Remarks("`<message>`")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task Say([Remainder] string text)
    {
        await Context.Message.ReplyAsync($"{Context.User.GetDisplayNameSanitized()} told me to say: {text}");
    }
}

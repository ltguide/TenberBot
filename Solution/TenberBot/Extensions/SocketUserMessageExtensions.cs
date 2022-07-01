using Discord;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class SocketUserMessageExtensions
{
    public static MessageReference GetReferenceTo(this SocketUserMessage socketUserMessage)
    {
        return new MessageReference(socketUserMessage.Id);
    }

    public static async Task<IUserMessage> ReplyToAsync(this SocketUserMessage socketUserMessage, string message = null!, bool isTTS = false, Embed embed = null!, RequestOptions options = null!, AllowedMentions allowedMentions = null!, MessageComponent components = null!, ISticker[] stickers = null!, Embed[] embeds = null!)
    {
        return await socketUserMessage.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, socketUserMessage.GetReferenceTo(), components, stickers, embeds).ConfigureAwait(false);
    }
}

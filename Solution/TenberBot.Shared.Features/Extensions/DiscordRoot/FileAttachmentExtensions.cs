using Discord;

namespace TenberBot.Shared.Features.Extensions.DiscordRoot;

public static class FileAttachmentExtensions
{
    public static byte[] GetBytes(this FileAttachment fileAttachment)
    {
        using var memoryStream = new MemoryStream();

        fileAttachment.Stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
}

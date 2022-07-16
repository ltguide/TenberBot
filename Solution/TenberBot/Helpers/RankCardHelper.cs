using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TenberBot.Data.Models;
using TenberBot.Extensions.ImageSharp;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

namespace TenberBot.Helpers;

public class RankCardHelper
{
    public static MemoryStream GetStream(RankCard card, SocketGuild guild, SocketUser user, UserLevel userLevel, byte[]? myAvatar, byte[]? userAvatar)
    {
        var memoryStream = new MemoryStream();

        using (var img = Image.Load(card.Data, out IImageFormat format))
        {
            img.Mutate(ctx => ctx.AddRankData(card, guild, user, userLevel));

            if (myAvatar != null)
            {
                using var myAvatarImage = Image.Load(myAvatar);
                myAvatarImage.Mutate(ctx => ctx.Resize(60, 60).BackgroundColor(Color.Black));

                img.Mutate(ctx => ctx.DrawImage(myAvatarImage, new Point(140, 190), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            if (userAvatar != null)
            {
                using var userAvatarImage = Image.Load(userAvatar);
                userAvatarImage.Mutate(ctx => ctx.Resize(160, 160).ApplyRoundedCorners(80));

                img.Mutate(ctx => ctx.DrawImage(userAvatarImage, new Point(15, 40), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            img.Save(memoryStream, format);
        }

        return memoryStream;
    }
}

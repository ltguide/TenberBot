using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using TenberBot.Data.Models;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Extensions.ImageSharp;

public static class RankCardImageSharpExtensions
{
    public static IImageProcessingContext AddRankData(
        this IImageProcessingContext processingContext,
        SocketGuild guild,
        SocketUser user,
        UserLevel userLevel)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var fontCollection = new FontCollection();

        var segoeui = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeui.ttf")!);
        var segoeuii = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuii.ttf")!);
        var segoeuib = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuib.ttf")!);

        var font30 = segoeui.CreateFont(30);
        var font40 = segoeui.CreateFont(40);
        var font40b = segoeui.CreateFont(40, FontStyle.Bold);
        var font40i = segoeuii.CreateFont(40, FontStyle.Italic);
        var font50b = segoeuib.CreateFont(50, FontStyle.Bold);

        var messageScale = (float)(userLevel.MessageExperience / userLevel.NextLevelMessageExperience);
        var voiceScale = (float)(userLevel.VoiceExperience / userLevel.NextLevelVoiceExperience);

        return processingContext
            // Guild Name
            .DrawText(
                new TextOptions(font40i)
                {
                    Origin = new PointF(1070, 3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                guild.Name,
                Color.White
            )
            // User Name
            .DrawText(
                user.GetDisplayName(),
                font50b,
                Color.White,
                new PointF(355, 80)
            )
            // Message Rank
            .DrawText(
                new TextOptions(font40b)
                {
                    Origin = new PointF(405, 210),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.MessageRank.ToString(),
                Color.White
            )
            // Message Level
            .DrawText(
                userLevel.MessageLevel.ToString(),
                font40,
                Color.White,
                new PointF(622, 175)
            )
            // Message Total Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(1010, 187),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.MessageExperience:N2} exp",
                Color.White
            )
            // Message fill
            .Fill(
                Color.DarkRed,
                new RectangleF(541, 230, 470 * messageScale, 36)
            )
            // Message Current Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(770, 226),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.MessageExperience - userLevel.CurrentLevelMessageExperience:N2} / {userLevel.NextLevelMessageExperience - userLevel.CurrentLevelMessageExperience:N0}",
                Color.White
            )
            // Voice Rank
            .DrawText(
                new TextOptions(font40b)
                {
                    Origin = new PointF(405, 315),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.VoiceRank.ToString(),
                Color.White
            )
            // Voice Level
            .DrawText(
                userLevel.VoiceLevel.ToString(),
                font40,
                Color.White,
                new PointF(622, 280)
            )
            // Voice Total Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(1010, 292),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.VoiceExperience:N2} exp",
                Color.White
            )
            // Voice fill
            .Fill(
                Color.DarkRed,
                new RectangleF(543, 335, 470 * voiceScale, 35)
            )
            // Voice Current Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(770, 330),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.VoiceExperience - userLevel.CurrentLevelVoiceExperience:N2} / {userLevel.NextLevelVoiceExperience - userLevel.CurrentLevelVoiceExperience:N0}",
                Color.White
            );
    }
}

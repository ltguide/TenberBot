using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using TenberBot.Data.Models;
using TenberBot.Data.Settings.Server;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Extensions.ImageSharp;

public static class RankCardImageSharpExtensions
{
    public static IImageProcessingContext AddRankData(
        this IImageProcessingContext processingContext,
        RankServerSettings settings,
        SocketGuild guild,
        SocketUser user,
        UserLevel userLevel)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var fontCollection = new FontCollection();

        var segoeui = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeui.ttf")!);
        var segoeuii = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuii.ttf")!);
        var segoeuib = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuib.ttf")!);
        var seguiemj = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.seguiemj.ttf")!);

        var font30 = segoeui.CreateFont(30);
        var font40 = segoeui.CreateFont(40);
        var font40b = segoeui.CreateFont(40, FontStyle.Bold);
        var font40i = segoeuii.CreateFont(40, FontStyle.Italic);
        var font50b = segoeuib.CreateFont(50, FontStyle.Bold);

        var fillColor = Color.ParseHex(settings.BackgroundFill);

        return processingContext
            // Guild Name
            .DrawText(
                new TextOptions(font40i)
                {
                    Origin = new PointF(1070, 3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FallbackFontFamilies = new List<FontFamily>() { seguiemj, }
                },
                guild.Name,
                Color.White
            )
            // User Name
            .DrawText(
                new TextOptions(font50b)
                {
                    Origin = new PointF(355, 80),
                    FallbackFontFamilies = new List<FontFamily>() { seguiemj, }
                },
                user.GetDisplayName(),
                Color.White
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
                fillColor,
                new RectangleF(535, 230, 475 * (float)(userLevel.MessageExperienceAmountCurrentLevel / userLevel.MessageExperienceRequiredCurrentLevel), 36)
            )
            // Message Current Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(770, 226),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.MessageExperienceAmountCurrentLevel:N2} / {userLevel.MessageExperienceRequiredCurrentLevel:N0}",
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
                fillColor,
                new RectangleF(539, 335, 475 * (float)(userLevel.VoiceExperienceAmountCurrentLevel / userLevel.VoiceExperienceRequiredCurrentLevel), 35)
            )
            // Voice Current Experience
            .DrawText(
                new TextOptions(font30)
                {
                    Origin = new PointF(770, 330),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.VoiceExperienceAmountCurrentLevel:N2} / {userLevel.VoiceExperienceRequiredCurrentLevel:N0}",
                Color.White
            );
    }
}

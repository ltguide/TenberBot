﻿using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.ImageGeneration;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Features.ExperienceFeature.Extensions.ImageSharp;

public static class RankCardImageSharpExtensions
{
    public static IImageProcessingContext AddRankData(
        this IImageProcessingContext processingContext,
        RankCard card,
        SocketGuild guild,
        SocketUser user,
        UserLevel userLevel)
    {
        var fonts = new ImageFonts();

        var font24 = fonts.Segoeui.CreateFont(24);
        var font24b = fonts.Segoeui.CreateFont(24, FontStyle.Bold);
        var font28 = fonts.Segoeui.CreateFont(28);
        var font28b = fonts.Segoeui.CreateFont(28, FontStyle.Bold);
        var font24i = fonts.Segoeui.CreateFont(24, FontStyle.Italic);
        var font32b = fonts.Segoeui.CreateFont(32, FontStyle.Bold);

        return processingContext
            // Guild Name
            .DrawText(
                new TextOptions(font24i)
                {
                    Origin = new PointF(795, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FallbackFontFamilies = fonts.FallbackFontFamilies,
                },
                guild.Name,
                Color.ParseHex(card.GuildColor)
            )
            // User Name
            .DrawText(
                new TextOptions(font32b)
                {
                    Origin = new PointF(210, 40),
                    FallbackFontFamilies = fonts.FallbackFontFamilies,
                },
                user.GetDisplayName(),
                Color.ParseHex(card.UserColor)
            )
            // Role Name
            .DrawText(
                new TextOptions(font24)
                {
                    Origin = new PointF(5, 268),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FallbackFontFamilies = fonts.FallbackFontFamilies,
                },
                card.Name,
                Color.ParseHex(card.RoleColor)
            )
            // Message Rank
            .DrawText(
                new TextOptions(font28b)
                {
                    Origin = new PointF(318, 132),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.MessageRank.ToString(),
                Color.ParseHex(card.RankColor)
            )
            // Message Level
            .DrawText(
                userLevel.MessageLevel.ToString(),
                font28b,
                Color.ParseHex(card.LevelColor),
                new PointF(425, 96)
            )
            // Message Total Experience
            .DrawText(
                new TextOptions(font28)
                {
                    Origin = new PointF(780, 96),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.MessageExperience:N2} XP",
                Color.ParseHex(card.ExperienceColor)
            )
            // Message fill
            .Fill(
                Color.ParseHex(card.ProgressFill),
                new RectangleF(364, 138, 414 * (float)(userLevel.MessageExperienceAmountCurrentLevel / userLevel.MessageExperienceRequiredCurrentLevel), 30)
            )
            // Message Current Experience
            .DrawText(
                new TextOptions(font24b)
                {
                    Origin = new PointF(571, 134),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.MessageExperienceAmountCurrentLevel:N2} / {userLevel.MessageExperienceRequiredCurrentLevel:N0}",
                Brushes.Solid(Color.ParseHex(card.ProgressColor)),
                Pens.Solid(Color.ParseHex(card.ProgressFill), 1f)
            )
            // Voice Rank
            .DrawText(
                new TextOptions(font28b)
                {
                    Origin = new PointF(318, 218),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.VoiceRank.ToString(),
                Color.ParseHex(card.RankColor)
            )
            // Voice Level
            .DrawText(
                userLevel.VoiceLevel.ToString(),
                font28b,
                Color.ParseHex(card.LevelColor),
                new PointF(425, 182)
            )
            // Voice Total Experience
            .DrawText(
                new TextOptions(font28)
                {
                    Origin = new PointF(780, 182),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.VoiceExperience:N2} XP",
                Color.ParseHex(card.ExperienceColor)
            )
            // Voice fill
            .Fill(
                Color.ParseHex(card.ProgressFill),
                new RectangleF(364, 224, 414 * (float)(userLevel.VoiceExperienceAmountCurrentLevel / userLevel.VoiceExperienceRequiredCurrentLevel), 30)
            )
            // Voice Current Experience
            .DrawText(
                new TextOptions(font24b)
                {
                    Origin = new PointF(571, 220),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.VoiceExperienceAmountCurrentLevel:N2} / {userLevel.VoiceExperienceRequiredCurrentLevel:N0}",
                Brushes.Solid(Color.ParseHex(card.ProgressColor)),
                Pens.Solid(Color.ParseHex(card.ProgressFill), 1.4f)
            );
    }
}

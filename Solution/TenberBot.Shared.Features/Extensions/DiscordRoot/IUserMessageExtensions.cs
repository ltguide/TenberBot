﻿using Discord;

namespace TenberBot.Shared.Features.Extensions.DiscordRoot;

public static class IUserMessageExtensions
{
    public static void DeleteSoon(this IUserMessage message, TimeSpan? timeSpan = null)
    {
        _ = Task.Delay(timeSpan ?? TimeSpan.FromSeconds(5))
            .ContinueWith(_ => message.DeleteAsync());
    }
}

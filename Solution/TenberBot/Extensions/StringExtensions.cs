using Discord;

namespace TenberBot.Extensions;

public static class StringExtensions
{
    public static string SanitizeMD(this string value)
    {
        return Format.Sanitize(value);
    }

    public static string GetUserMention(this string value)
    {
        return $"<@{value}>";
    }

    public static string GetChannelMention(this string value)
    {
        return $"<#{value}>";
    }

    public static string GetRoleMention(this string value)
    {
        return $"<@&{value}>";
    }

    public static IEmote? AsIEmote(this string value)
    {
        if (Emote.TryParse(value, out var emote))
            return emote;

        if (Emoji.TryParse(value, out var emoji))
            return emoji;

        return null;
    }

    public static IEnumerable<string> ChunkByLines(this string value, int size)
    {
        if (value.Length <= size)
            yield return value;
        else
        {
            var startIndex = 0;

            do
            {
                var lastIndex = value.LastIndexOf('\n', startIndex + size - 1, size);
                //Console.WriteLine($"inside do; startIndex:{startIndex} lastIndex:{lastIndex}");

                if (lastIndex == -1)
                {
                    yield return value.Substring(startIndex, size);
                    startIndex += size;
                }
                else
                {
                    yield return value.Substring(startIndex, lastIndex - startIndex + 1);
                    startIndex = lastIndex + 1;
                }
            }
            while (startIndex + size < value.Length);

            //Console.WriteLine($"outside; startIndex:{startIndex}");

            if (startIndex < value.Length)
                yield return value[startIndex..];
        }
    }
}

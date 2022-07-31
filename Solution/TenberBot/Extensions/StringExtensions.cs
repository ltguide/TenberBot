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

    public static IEnumerable<string> ChunkBy(this string value, int size, string delimiter = "\n")
    {
        if (value.Length <= size)
            yield return value;
        else
        {
            var startIndex = 0;
            var length = delimiter.Length;

            do
            {
                var lastIndex = value.LastIndexOf(delimiter, startIndex + size - 1, size);
                //Console.WriteLine($"do; startIndex:{startIndex} lastIndex:{lastIndex}");

                if (lastIndex == -1)
                {
                    yield return value.Substring(startIndex, size);
                    startIndex += size;
                }
                else
                {
                    yield return value.Substring(startIndex, lastIndex - startIndex + length);
                    startIndex = lastIndex + length;
                }
            }
            while (startIndex + size < value.Length);

            //Console.WriteLine($"outside; startIndex:{startIndex}");

            if (startIndex < value.Length)
                yield return value[startIndex..];
        }
    }
}

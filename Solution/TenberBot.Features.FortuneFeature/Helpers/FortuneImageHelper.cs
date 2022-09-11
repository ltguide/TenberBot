using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.ImageGeneration;
using Image = SixLabors.ImageSharp.Image;

namespace TenberBot.Features.FortuneFeature.Helpers;

public static class FortuneImageHelper
{
    public static MemoryStream GetStream(Visual visual, string message)
    {
        var memoryStream = new MemoryStream();

        using (var img = Image.Load(visual.Data, out IImageFormat format))
        {
            //img.Mutate(ctx => ctx.Fill(Color.PaleTurquoise, new RectangleF(330, 540, 400, 400)));

            img.Mutate(ctx => ctx.DrawBoundedTextAt(message, new Size(400, 400), new Point(330, 540), new ImageFonts().Harrington.CreateFont(55), Color.DeepPink));

            img.Save(memoryStream, format);
        }

        return memoryStream;
    }

    public static IImageProcessingContext DrawBoundedTextAt(this IImageProcessingContext processingContext, string text, Size size, Point point, Font font, Color color, float? minFontSize = null)
    {
        var textOptions = new TextOptions(font)
        {
            Origin = new PointF(point.X + (size.Width / 2), point.Y + (size.Height / 2)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = size.Width,
            TextAlignment = TextAlignment.Center,
            LineSpacing = .7f,
            WordBreaking = WordBreaking.Normal,
        };

        minFontSize ??= font.Size * .6f;

        while (true)
        {
            var q = TextMeasurer.Measure(text, textOptions);

            //Console.WriteLine($"{textOptions.Font.Size} / {q.Width} x {q.Height}");

            if (q.Height <= size.Height && q.Width <= size.Width)
                break;

            textOptions.Font = new Font(font.Family, textOptions.Font.Size - 2.5f);

            if (textOptions.Font.Size <= minFontSize && textOptions.WordBreaking == WordBreaking.Normal)
            {
                //Console.WriteLine($"{textOptions.Font.Size} <= {minFont}");

                textOptions.WordBreaking = WordBreaking.BreakAll;
                textOptions.Font = new Font(font.Family, font.Size);
            }
        }

        return processingContext
            .DrawText(
                textOptions,
                text,
                Brushes.Solid(color),
                Pens.Solid(Color.DarkGray, 1f)
            );
    }
}

//using SixLabors.Fonts;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Drawing.Processing;
//using SixLabors.ImageSharp.Processing;

//namespace TenberBot.Shared.ImageGeneration.Extensions;

//public static class ImageSharpWatermarkExtensions
//{
//    public static IImageProcessingContext ApplyScalingWaterMarkSimple(this IImageProcessingContext processingContext, Font font, string text, Color color)
//    {
//        var imageSize = processingContext.GetCurrentSize();
//        var textSize = TextMeasurer.Measure(text, new TextOptions(font));

//        float scalingFactor = Math.Min(imageSize.Width / textSize.Width, imageSize.Height / textSize.Height);

//        return processingContext
//            .DrawText(
//                new TextOptions(new Font(font, scalingFactor * font.Size))
//                {
//                    Origin = new PointF(imageSize.Width / 2, imageSize.Height / 2),
//                    HorizontalAlignment = HorizontalAlignment.Center,
//                    VerticalAlignment = VerticalAlignment.Center,
//                },
//                text,
//                color
//            );
//    }

//    public static IImageProcessingContext ApplyScalingWaterMarkWordWrap(this IImageProcessingContext processingContext, Font font, string text, Color color, float padding)
//    {
//        var imageSize = processingContext.GetCurrentSize();
//        float targetWidth = imageSize.Width - (padding * 2);
//        float targetHeight = imageSize.Height - (padding * 2);

//        float targetMinHeight = imageSize.Height - (padding * 3); // must be with in a margin width of the target height

//        // now we are working i 2 dimensions at once and can't just scale because it will cause the text to
//        // reflow we need to just try multiple times

//        var scaledFont = font;
//        var s = new FontRectangle(0, 0, float.MaxValue, float.MaxValue);

//        float scaleFactor = (scaledFont.Size / 2); // every time we change direction we half this size
//        int trapCount = (int)scaledFont.Size * 2;
//        if (trapCount < 10)
//        {
//            trapCount = 10;
//        }

//        bool isTooSmall = false;

//        while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0)
//        {
//            if (s.Height > targetHeight)
//            {
//                if (isTooSmall)
//                    scaleFactor /= 2;

//                scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
//                isTooSmall = false;
//            }

//            if (s.Height < targetMinHeight)
//            {
//                if (!isTooSmall)
//                    scaleFactor /= 2;


//                scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
//                isTooSmall = true;
//            }
//            trapCount--;

//            s = TextMeasurer.Measure(text, new TextOptions(scaledFont)
//            {
//                WrappingLength = targetWidth
//            });
//        }

//        var center = new PointF(padding, imageSize.Height / 2);
//        var textGraphicOptions = new TextOptions(scaledFont)
//        {
//            Origin = center,
//            HorizontalAlignment = HorizontalAlignment.Left,
//            VerticalAlignment = VerticalAlignment.Center,
//            WrappingLength = targetWidth,
//        };
//        return processingContext.DrawText(textGraphicOptions, text, color);
//    }
//}

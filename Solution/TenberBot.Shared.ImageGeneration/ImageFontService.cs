using SixLabors.Fonts;
using System.Reflection;

namespace TenberBot.Shared.ImageGeneration;

public class ImageFonts
{
    public FontFamily Segoeui { get; init; }

    public FontFamily Harrington { get; init; }

    public List<FontFamily> FallbackFontFamilies { get; init; }

    public ImageFonts()
    {
        var assembly = Assembly.GetExecutingAssembly()!;
        var assemblyName = assembly.GetName().Name;

        var fontCollection = new FontCollection();

        Segoeui = fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.segoeui.ttf")!);

        fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.segoeuii.ttf")!);
        fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.segoeuib.ttf")!);

        Harrington = fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.HARNGTON.TTF")!);

        FallbackFontFamilies = new List<FontFamily>() {
            fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.seguiemj.ttf")!),
            fontCollection.Add(assembly.GetManifestResourceStream($"{assemblyName}.Fonts.seguihis.ttf")!)
        };
    }
}

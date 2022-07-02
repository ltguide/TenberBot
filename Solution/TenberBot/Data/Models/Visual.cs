using Discord;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("Visuals")]
public class Visual
{
    [Key]
    public int VisualId { get; set; }

    public VisualType VisualType { get; set; }

    public string Filename { get; set; } = "";

    public string Url { get; set; } = "";

    public byte[] Data { get; set; } = Array.Empty<byte>();

    [NotMapped]
    public string AttachmentFilename => $"{VisualId}_{Filename}";

    [NotMapped]
    public Stream Stream
    {
        get { return new MemoryStream(Data); }
        set
        {
            using var memoryStream = new MemoryStream();

            value.CopyTo(memoryStream);

            Data = memoryStream.ToArray();
        }
    }

    public Visual()
    {
    }

    public Visual(FileAttachment value)
    {
        Filename = value.FileName;
        Stream = value.Stream;
    }
}

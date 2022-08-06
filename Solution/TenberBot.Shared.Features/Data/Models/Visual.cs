using Discord;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Shared.Features.Data.Enums;

namespace TenberBot.Shared.Features.Data.Models;

[Table("Visuals")]
[Index(nameof(VisualType))]
public class Visual
{
    [Key]
    public int VisualId { get; set; }

    public VisualType VisualType { get; set; }

    public string Filename { get; set; } = "";

    public string Url { get; set; } = "";

    public byte[] Data { get; set; } = Array.Empty<byte>();

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

    public FileAttachment AsAttachment() => new(Stream, AttachmentFilename);
}

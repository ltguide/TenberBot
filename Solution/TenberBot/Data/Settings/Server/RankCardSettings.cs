namespace TenberBot.Data.Settings.Server;

public class RankCardSettings
{
    public string Role { get; set; } = "";

    public string ImageName { get; set; } = "";

    public byte[]? ImageData { get; set; }

    public string GuildColor { get; set; } = "FFFFFFFF";

    public string UserColor { get; set; } = "FFFFFFFF";

    public string RankColor { get; set; } = "FFFFFFFF";

    public string LevelColor { get; set; } = "FFFFFFFF";

    public string ExperienceColor { get; set; } = "FFFFFFFF";

    public string ProgressColor { get; set; } = "FFFFFFFF";

    public string ProgressFill { get; set; } = "000000FF";
}

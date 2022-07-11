﻿using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Server;

[ServerSettings("rank")]
public class RankServerSettings
{
    public byte[]? BackgroundData { get; set; }

    public string BackgroundName { get; set; } = "";

    public string BackgroundFill { get; set; } = "000000FF";
}
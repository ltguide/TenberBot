﻿using Discord.Interactions;

namespace TenberBot.Modals.SprintSnippet;

public class SprintSnippetAddModal : IModal
{
    public string Title => "Add Sprint Snippet: ";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}

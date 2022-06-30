using Discord;
using Discord.Interactions;

namespace TenberBot.Modules;

public class TestInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("sprint", "Create a sprint that others can join! Get stuff done!")]
    public async Task Sprint(TimeSpan duration, string task)
    {
        //duration = duration.Duration(); // absolute to cover negative

        await RespondAsync($"{duration} - {task}", ephemeral: true);
    }

    [SlashCommand("echo", "Echo an input")]
    public async Task Echo([Summary(description: "this is a parameter description")] string input)
    {
        await RespondAsync($"echo {input}");
    }

    [SlashCommand("test", "moo")]
    public async Task Test([Summary(description: "this is a parameter description")] string input)
    {

        await RespondAsync(input, ephemeral: true);

        //await ReplyAsync(input);
    }

    [SlashCommand("food", "Tell us about your favorite food.")]
    public async Task Command()
    => await Context.Interaction.RespondWithModalAsync<FoodModal>("food_menu");


    [ModalInteraction("food_menu")]
    public async Task ModalResponse(FoodModal modal)
    {
        // Build the message to send.
        string message = "hey @everyone, I just learned " +
            $"{Context.User.Mention}'s favorite food is " +
            $"{modal.Food} because {modal.Reason}.";

        // Specify the AllowedMentions so we don't actually ping everyone.
        AllowedMentions mentions = new();
        mentions.AllowedTypes = AllowedMentionTypes.Users;

        // Respond to the modal.
        await RespondAsync(message, allowedMentions: mentions);
    }


    [ComponentInteraction("custom-id:*")]
    public async Task Play(string id)
    {

        await RespondAsync($"something: #{id}");
    }
}


public class FoodModal : IModal
{
    public string Title => "Fav Food";
    // Strings with the ModalTextInput attribute will automatically become components.
    [InputLabel("What??")]
    [ModalTextInput("food_name", placeholder: "Pizza", maxLength: 20)]
    public string Food { get; set; }

    // Additional paremeters can be specified to further customize the input.
    [InputLabel("Why??")]
    [ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
    public string Reason { get; set; }

    //[InputLabel("Duration??")]
    //[ModalTextInput("food_duration", TextInputStyle.Short, "Kuz it's tasty", maxLength: 20)]
    //public TimeSpan Duration { get; set; }
}

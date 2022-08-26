using Discord;
using Discord.Interactions;
using TenberBot.Shared.Features;

namespace TenberBot.Features.BotSettingFeature.Handlers;

public class VisualTypeAutocompleteHandler : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var input = autocompleteInteraction.Data.Options.First(x => x.Name == parameter.Name).Value as string ?? "";

        var results = SharedFeatures.Visuals.Where(x => x.Contains(input, StringComparison.CurrentCultureIgnoreCase)).Select(x => new AutocompleteResult(x, x));

        return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25)));
    }
}

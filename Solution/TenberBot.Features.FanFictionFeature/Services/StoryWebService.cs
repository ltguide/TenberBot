using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TenberBot.Features.FanFictionFeature.Data.POCO;

namespace TenberBot.Features.FanFictionFeature.Services;

public class StoryWebService
{
    private readonly IMemoryCache memoryCache;
    private readonly HttpClient client;
    private readonly ILogger<StoryWebService> logger;

    public StoryWebService(
        IMemoryCache memoryCache,
        HttpClient client,
        ILogger<StoryWebService> logger)
    {
        this.memoryCache = memoryCache;
        this.client = client;
        this.logger = logger;
    }

    public async Task<Story?> GetAO3(string url)
    {
        var key = $"{GetType()}, {url}";

        if (memoryCache.TryGetValue<Story>(key, out var value))
            return value;

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Cookie", "view_adult=true");

        try
        {
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode == false)
                return null;

            if (AO3Story.TryParse(url, await response.Content.ReadAsStringAsync(), out var story))
                return memoryCache.Set(key, story, TimeSpan.FromMinutes(10));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, $"Failed to GET {url}");
        }

        return null;
    }
}

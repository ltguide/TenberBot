using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TenberBot.Shared.Features.Extensions.DiscordRoot;

namespace TenberBot.Shared.Features.Services;

public class VisualWebService
{
    private readonly IMemoryCache memoryCache;
    private readonly HttpClient client;
    private readonly ILogger<VisualWebService> logger;

    public VisualWebService(
        IMemoryCache memoryCache,
        HttpClient client,
        ILogger<VisualWebService> logger)
    {
        this.memoryCache = memoryCache;
        this.client = client;
        this.logger = logger;
    }

    public async Task<FileAttachment?> GetFileAttachment(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        try
        {
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode == false)
                return null;

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "";

            if (mediaType.StartsWith("image/") == false)
            {
                logger.LogInformation($"Got {mediaType} (not an image) from: {url}");
                return null;
            }

            return new FileAttachment(await response.Content.ReadAsStreamAsync(), GetFilename(response));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, $"Failed to GET {url}");
            return null;
        }
    }

    public async Task<byte[]?> GetBytes(string url, TimeSpan duration)
    {
        var key = $"WebService, {url}";

        if (memoryCache.TryGetValue<byte[]>(key, out var value))
            return value;

        var fileAttachment = await GetFileAttachment(url);
        if (fileAttachment == null)
            return null;

        return memoryCache.Set(key, fileAttachment.Value.GetBytes(), duration);
    }

    private static string GetFilename(HttpResponseMessage response)
    {
        return response.Content.Headers.ContentDisposition?.FileNameStar ?? Path.GetFileName(response.RequestMessage?.RequestUri?.AbsolutePath ?? "unknown");
    }
}

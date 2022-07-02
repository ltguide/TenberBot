using Discord;
namespace TenberBot.Services;

public class WebService
{
    private readonly HttpClient client;
    private readonly ILogger<WebService> logger;

    public WebService(
        HttpClient client,
        ILogger<WebService> logger)
    {
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

    private static string GetFilename(HttpResponseMessage response)
    {
        return response.Content.Headers.ContentDisposition?.FileNameStar ?? Path.GetFileName(response.RequestMessage?.RequestUri?.AbsolutePath ?? "unknown");
    }
}

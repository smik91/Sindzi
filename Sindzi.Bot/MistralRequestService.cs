using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Sindzi.Common.Options;

namespace Sindzi.Bot;

public class MistralRequestService
{
    private readonly HttpClient HttpClient = new();
    private readonly MistralOptions _options;

    public MistralRequestService(IOptions<MistralOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GetResponseAsync(string userInput)
    {
        var requestBody = new
        {
            random_seed = 5,
            messages = new[]
            {
                    new { role = "user", content = userInput }
                },
            response_format = new
            {
                type = "text"
            },
            agent_id = _options.AgentId
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        try
        {
            var response = await HttpClient.PostAsync(_options.ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return HandleErrorResponse(response.StatusCode, errorResponse);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return ExtractResponseContent(responseBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during request data from Mistral API: {ex.Message}");
            return string.Empty;
        }
    }

    private string ExtractResponseContent(string responseBody)
    {
        using var jsonDoc = JsonDocument.Parse(responseBody);
        var root = jsonDoc.RootElement;

        var content = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    private string HandleErrorResponse(System.Net.HttpStatusCode statusCode, string errorResponse)
    {
        if (statusCode == System.Net.HttpStatusCode.UnprocessableEntity)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(errorResponse);
                var root = jsonDoc.RootElement;

                var errorDetail = root
                    .GetProperty("detail")[0]
                    .GetProperty("msg")
                    .GetString();

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        return string.Empty;
    }
}

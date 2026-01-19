using System.Text.Json;
using ExternalApiCache.Models;

namespace ExternalApiCache.Services;

public class ExternalApiService
{
    private readonly HttpClient _httpClient;

    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Character> FetchCharacterById(long characterId)
    {
        string apiUrl = $"character/{characterId}";

        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        string jsonData = await response.Content.ReadAsStringAsync();
        var result = MapApiResultToCharacterModel(jsonData);
        return result;
    }

    public Character MapApiResultToCharacterModel(string jsonData)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<Character>(jsonData, options)
                ?? throw new JsonException("Failed to deserialize Character");
    }
}

using ExternalApiCache.Models;

namespace ExternalApiCache.Services;

public class ExternalApiService
{
    public async Task<Character> FetchCharacterById(long characterId)
    {
        HttpClient externalApi = new HttpClient();
        string apiUrl = $"https://rickandmortyapi.com/api/character/{characterId}";

        try
        {
            HttpResponseMessage response = await externalApi.GetAsync(apiUrl);

            string jsonData = await response.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {

            throw;
        }
        return new Character();
    }
}

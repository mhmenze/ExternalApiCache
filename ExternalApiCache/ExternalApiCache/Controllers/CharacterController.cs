using ExternalApiCache.Models;
using ExternalApiCache.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExternalApiCache.Controllers;

[ApiController]
[Route("[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ILogger<CharacterController> _logger;
    private readonly ExternalApiService _apiService;

    public CharacterController(
            ILogger<CharacterController> logger,
            ExternalApiService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    [HttpGet(Name = "GetCharacter")]
    public async Task<ActionResult<Character>> GetCharacterById(long characterId)
    {
        var results = await _apiService.FetchCharacterById(characterId);
        return results;
    }
}

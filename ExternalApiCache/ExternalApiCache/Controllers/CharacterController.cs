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
    private readonly LocalCacheService _localService;

    public CharacterController(
            ILogger<CharacterController> logger,
            ExternalApiService apiService,
            LocalCacheService localService)
    {
        _logger = logger;
        _apiService = apiService;
        _localService = localService;
    }

    [HttpGet("{characterId}", Name = "GetCharacterById")]
    public async Task<ActionResult<Character>> GetCharacterById(long characterId)
    {
        var dbResult = await _localService.GetCharacterFromLocalDbById(characterId);

        if (dbResult != null)
            return Ok(dbResult);

        var apiResult = await _apiService.FetchCharacterById(characterId);

        await _localService.InsertCharacterToDb(apiResult);

        return Ok(apiResult);
    }

    [HttpGet("all", Name = "GetAllCharacters")]
    public async Task<ActionResult<IEnumerable<Character>>> GetAllCharacters()
    {
        var results = await _localService.GetAllCharactersFromlocalDb();
        return Ok(results);
    }
}

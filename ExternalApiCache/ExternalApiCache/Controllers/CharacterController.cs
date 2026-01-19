using System.Text.Json;
using ExternalApiCache.Models;
using ExternalApiCache.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
        if (characterId <= 0)
        {
            return BadRequest("ID must be a positive whole number");
        }
        try
        {
            var dbResult = await _localService.GetCharacterFromLocalDbById(characterId);

            if (dbResult != null)
            {
                _logger.LogInformation("Character found in Cache DB");
                return Ok(dbResult);
            }

            _logger.LogInformation("Character not in DB, fetching from API");

            var apiResult = await _apiService.FetchCharacterById(characterId);

            await _localService.InsertCharacterToDb(apiResult);

            return Ok(apiResult);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch character from external API");
            return StatusCode(503, "External API unavailable");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize API response for character");
            return StatusCode(502, "Invalid response");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error");
            return StatusCode(500, "Database error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting character");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("all", Name = "GetAllCharacters")]
    public async Task<ActionResult<IEnumerable<Character>>> GetAllCharacters()
    {
        try
        {
            var results = await _localService.GetAllCharactersFromLocalDb();
            return Ok(results);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error");
            return StatusCode(500, "Database error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting character");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}

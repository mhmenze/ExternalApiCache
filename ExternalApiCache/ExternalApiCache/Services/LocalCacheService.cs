using System.Data;
using ExternalApiCache.Models;
using Microsoft.Data.SqlClient;

namespace ExternalApiCache.Services;

public class LocalCacheService
{
    private readonly string _connectionString;

    public LocalCacheService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string not found");
    }

    public async Task<Character?> GetCharacterFromLocalDbById(long characterId)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("RM.GetCharacterById", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Id", characterId);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var character = MapCharacterFromReader(reader);
        reader.Close();

        character.Episode = await GetEpisodesForCharacter(characterId, connection);
        return character;
    }

    public async Task<List<Character>> GetAllCharactersFromlocalDb()
    {
        var characters = new List<Character>();
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("RM.GetAllCharacters", connection);
        command.CommandType = CommandType.StoredProcedure;

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            characters.Add(MapCharacterFromReader(reader));
        }
        reader.Close();
        foreach (var character in characters)
        {
            character.Episode = await GetEpisodesForCharacter(character.Id, connection);
        }

        return characters;
    }

    private Character MapCharacterFromReader(SqlDataReader reader)
    {
        return new Character
        {
            Id = Convert.ToInt64(reader["Id"]),
            Name = reader["Name"]?.ToString() ?? "",
            Status = reader["Status"]?.ToString() ?? "",
            Species = reader["Species"]?.ToString() ?? "",
            Type = reader["Type"]?.ToString() ?? "",
            Gender = reader["Gender"]?.ToString() ?? "",
            Image = reader["Image"]?.ToString() ?? "",
            Url = reader["Url"]?.ToString() ?? "",
            Created = reader["Created"]?.ToString() ?? "",
            Origin = new CharacterOrigin
            {
                Name = reader["OriginName"]?.ToString() ?? "",
                Url = reader["OriginUrl"]?.ToString() ?? ""
            },
            Location = new CharacterLocation
            {
                Name = reader["LocationName"]?.ToString() ?? "",
                Url = reader["LocationUrl"]?.ToString() ?? ""
            }
        };
    }

    private async Task<List<string>> GetEpisodesForCharacter(long characterId, SqlConnection connection)
    {
        var episodes = new List<string>();

        using var command = new SqlCommand("RM.GetEpisodesByCharacterId", connection);
        command.Parameters.AddWithValue("@CharacterId", characterId);
        command.CommandType = CommandType.StoredProcedure;

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            episodes.Add(reader.GetString(0));
        }

        return episodes;
    }

    public async Task InsertCharacterToDb(Character character)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var originCmd = new SqlCommand("RM.GetOrInsertOrigin", connection, transaction);
            originCmd.CommandType = CommandType.StoredProcedure;

            originCmd.Parameters.AddWithValue("@name", character.Origin.Name);
            originCmd.Parameters.AddWithValue("@url", character.Origin.Url);

            var originIdParam = new SqlParameter("@OriginId", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };
            originCmd.Parameters.Add(originIdParam);
            await originCmd.ExecuteNonQueryAsync();

            long originId = (long)originIdParam.Value;

            var locationCmd = new SqlCommand("RM.GetOrInsertLocation", connection, transaction);
            locationCmd.CommandType = CommandType.StoredProcedure;

            locationCmd.Parameters.AddWithValue("@name", character.Location.Name);
            locationCmd.Parameters.AddWithValue("@url", character.Location.Url);

            var locationIdParam = new SqlParameter("@LocationId", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };
            locationCmd.Parameters.Add(locationIdParam);
            await locationCmd.ExecuteNonQueryAsync();

            long locationId = (long)locationIdParam.Value;

            var characterCmd = new SqlCommand("RM.InsertCharacter", connection, transaction);
            characterCmd.CommandType = CommandType.StoredProcedure;

            characterCmd.Parameters.AddWithValue("@name", character.Name);
            characterCmd.Parameters.AddWithValue("@status", character.Status);
            characterCmd.Parameters.AddWithValue("@id", character.Id);
            characterCmd.Parameters.AddWithValue("@species", character.Species);
            characterCmd.Parameters.AddWithValue("@type", character.Type);
            characterCmd.Parameters.AddWithValue("@gender", character.Gender);
            characterCmd.Parameters.AddWithValue("@image", character.Image);
            characterCmd.Parameters.AddWithValue("@url", character.Url);
            characterCmd.Parameters.AddWithValue("@created", character.Created);
            characterCmd.Parameters.AddWithValue("@originId", originId);
            characterCmd.Parameters.AddWithValue("@locationId", locationId);

            await characterCmd.ExecuteNonQueryAsync();

            foreach (var ep in character.Episode)
            {
                var epCmd = new SqlCommand("RM.InsertCharacterEpisode", connection, transaction);
                epCmd.CommandType = CommandType.StoredProcedure;

                epCmd.Parameters.AddWithValue("@cid", character.Id);
                epCmd.Parameters.AddWithValue("@url", ep);

                await epCmd.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

}

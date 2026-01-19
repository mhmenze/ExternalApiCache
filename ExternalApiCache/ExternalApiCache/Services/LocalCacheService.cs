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

    public async Task InsertCharacterToDb(Character character)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var originCmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM RM.Origin WHERE Url = @url)
              INSERT INTO RM.Origin (Name, Url) VALUES (@name, @url);
              SELECT OriginId FROM RM.Origin WHERE Url = @url;",
                connection, transaction);

            originCmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = character.Origin.Name;
            originCmd.Parameters.Add("@url", SqlDbType.NVarChar, 2048).Value = character.Origin.Url;

            long originId = (long)await originCmd.ExecuteScalarAsync();

            var locationCmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM RM.Location WHERE Url = @url)
              INSERT INTO RM.Location (Name, Url) VALUES (@name, @url);
              SELECT LocationId FROM RM.Location WHERE Url = @url;",
                connection, transaction);

            locationCmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = character.Location.Name;
            locationCmd.Parameters.Add("@url", SqlDbType.NVarChar, 2048).Value = character.Location.Url;

            long locationId = (long)await locationCmd.ExecuteScalarAsync();

            var characterCmd = new SqlCommand(
                @"IF NOT EXISTS (SELECT 1 FROM RM.Character WHERE Id = @id)
              INSERT INTO RM.Character
              (Id, Name, Status, Species, Type, Gender, Image, Url, Created, OriginId, LocationId)
              VALUES (@id, @name, @status, @species, @type, @gender, @image, @url, @created, @originId, @locationId);",
                connection, transaction);

            characterCmd.Parameters.Add("@id", SqlDbType.BigInt).Value = character.Id;
            characterCmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = character.Name;
            characterCmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = character.Status;
            characterCmd.Parameters.Add("@species", SqlDbType.NVarChar, 100).Value = character.Species;
            characterCmd.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = character.Type;
            characterCmd.Parameters.Add("@gender", SqlDbType.NVarChar, 50).Value = character.Gender;
            characterCmd.Parameters.Add("@image", SqlDbType.NVarChar, 2048).Value = character.Image;
            characterCmd.Parameters.Add("@url", SqlDbType.NVarChar, 2048).Value = character.Url;
            characterCmd.Parameters.Add("@created", SqlDbType.NVarChar, 100).Value = character.Created;
            characterCmd.Parameters.Add("@originId", SqlDbType.BigInt).Value = originId;
            characterCmd.Parameters.Add("@locationId", SqlDbType.BigInt).Value = locationId;

            await characterCmd.ExecuteNonQueryAsync();

            foreach (var ep in character.Episode)
            {
                var epCmd = new SqlCommand(
                    @"IF NOT EXISTS (SELECT 1 FROM RM.CharacterEpisode WHERE CharacterId = @cid AND EpisodeUrl = @url)
                  INSERT INTO RM.CharacterEpisode (CharacterId, EpisodeUrl)
                  VALUES (@cid, @url);",
                    connection, transaction);

                epCmd.Parameters.Add("@cid", SqlDbType.BigInt).Value = character.Id;
                epCmd.Parameters.Add("@url", SqlDbType.NVarChar, 2048).Value = ep;

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

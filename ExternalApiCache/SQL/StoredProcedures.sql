USE ExternalApiCache;
GO

----------------------------------1-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[GetOrInsertOrigin]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[GetOrInsertOrigin]
END
GO
CREATE PROCEDURE RM.GetOrInsertOrigin
    @name NVARCHAR(255),
    @url NVARCHAR(2048),
    @OriginId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM RM.Origin WHERE Url = @url)
        SELECT @OriginId = OriginId FROM RM.Origin WHERE Url = @url;
    ELSE
    BEGIN
        INSERT INTO RM.Origin (Name, Url) VALUES (@name, @url);
        SELECT @OriginId = SCOPE_IDENTITY();
    END
END
GO

----------------------------------2-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[GetOrInsertLocation]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[GetOrInsertLocation]
END
GO

CREATE PROCEDURE RM.GetOrInsertLocation
    @name NVARCHAR(255),
    @url NVARCHAR(2048),
    @LocationId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM RM.Location WHERE Url = @url)
        SELECT @LocationId = LocationId FROM RM.Location WHERE Url = @url;
    ELSE
    BEGIN
        INSERT INTO RM.Location (Name, Url) VALUES (@name, @url);
        SELECT @LocationId = SCOPE_IDENTITY();
    END
END
GO

----------------------------------3-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[InsertCharacter]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[InsertCharacter]
END
GO

CREATE PROCEDURE RM.InsertCharacter
    @id BIGINT,
    @name NVARCHAR(255),
    @status NVARCHAR(50),
    @species NVARCHAR(100),
    @type NVARCHAR(100),
    @gender NVARCHAR(50),
    @image NVARCHAR(2048),
    @url NVARCHAR(2048),
    @created NVARCHAR(100),
    @originId BIGINT,
    @locationId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM RM.Character WHERE Id = @Id)
    BEGIN
        INSERT INTO RM.Character (
            Id, 
            Name, 
            Status, 
            Species, 
            Type, 
            Gender, 
            Image, 
            Url, 
            Created, 
            OriginId, 
            LocationId)

        VALUES (
            @id, 
            @name, 
            @status, 
            @species, 
            @type, 
            @gender, 
            @image, 
            @url, 
            @created, 
            @originId, 
            @locationId);
    END
END
GO

----------------------------------4-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[InsertCharacterEpisode]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[InsertCharacterEpisode]
END
GO

CREATE PROCEDURE RM.InsertCharacterEpisode
    @cid BIGINT,
    @url NVARCHAR(2048)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM RM.CharacterEpisode WHERE CharacterId = @cid AND EpisodeUrl = @url)
    BEGIN
        INSERT INTO RM.CharacterEpisode (CharacterId, EpisodeUrl)
        VALUES (@cid, @url);
    END
END
GO

----------------------------------5-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[GetCharacterById]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[GetCharacterById]
END
GO

CREATE PROCEDURE RM.GetCharacterById
    @Id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT C.*, 
           O.Name AS OriginName, O.Url AS OriginUrl,
           L.Name AS LocationName, L.Url AS LocationUrl
    FROM RM.Character C
    LEFT JOIN RM.Origin O ON C.OriginId = O.OriginId
    LEFT JOIN RM.Location L ON C.LocationId = L.LocationId
    WHERE C.Id = @Id;
END
GO

----------------------------------5-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[GetAllCharacters]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[GetAllCharacters]
END
GO

CREATE PROCEDURE RM.GetAllCharacters
AS
BEGIN
    SET NOCOUNT ON;

    SELECT C.*, 
           O.Name AS OriginName, O.Url AS OriginUrl,
           L.Name AS LocationName, L.Url AS LocationUrl
    FROM RM.Character C
    LEFT JOIN RM.Origin O ON C.OriginId = O.OriginId
    LEFT JOIN RM.Location L ON C.LocationId = L.LocationId;
END
GO

----------------------------------6-----------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects 
           WHERE id = OBJECT_ID(N'[RM].[GetEpisodesByCharacterId]') AND type = 'P')
BEGIN
    DROP PROCEDURE [RM].[GetEpisodesByCharacterId]
END
GO

CREATE PROCEDURE RM.GetEpisodesByCharacterId
    @CharacterId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT EpisodeUrl
    FROM RM.CharacterEpisode
    WHERE CharacterId = @CharacterId;
END
GO

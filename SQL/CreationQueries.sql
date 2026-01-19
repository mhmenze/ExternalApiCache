CREATE DATABASE [ExternalApiCache]
GO

USE [ExternalApiCache]
GO

CREATE SCHEMA RM
GO

CREATE TABLE [RM].[Origin] (
    [OriginId] BIGINT IDENTITY PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Url] NVARCHAR(2048) NOT NULL
)
GO

CREATE TABLE [RM].[Location] (
    [LocationId] BIGINT IDENTITY PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Url] NVARCHAR(2048) NOT NULL
)
GO

CREATE TABLE [RM].[Character] (
    [Id] BIGINT PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Status] NVARCHAR(50),
    [Species] NVARCHAR(100),
    [Type] NVARCHAR(100),
    [Gender] NVARCHAR(50),
    [Image] NVARCHAR(2048),
    [Url] NVARCHAR(2048),
    [Created] NVARCHAR(100),    
    [OriginId] BIGINT,
    [LocationId] BIGINT,

    CONSTRAINT FK_Character_Origin 
        FOREIGN KEY ([OriginId]) 
        REFERENCES RM.[Origin](OriginId),

    CONSTRAINT FK_Character_Location 
        FOREIGN KEY ([LocationId]) 
        REFERENCES [RM].[Location](LocationId)
)
GO

CREATE TABLE [RM].[CharacterEpisode] (
    [EpisodeId] BIGINT IDENTITY PRIMARY KEY,
    [CharacterId] BIGINT,
    [EpisodeUrl] NVARCHAR(2048) NOT NULL,

    CONSTRAINT FK_Episode_Character 
        FOREIGN KEY ([CharacterId])
        REFERENCES RM.[Character](Id)
)
GO
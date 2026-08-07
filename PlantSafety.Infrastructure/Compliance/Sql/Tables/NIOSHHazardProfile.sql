USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Table:         Compliance.NIOSHHazardProfile
-- Description:   NIOSH reference-data hazard profile for a chemical/agent
--                (Recommended Exposure Limits, health effects, controls, PPE
--                guidance, atmospheric monitoring, ergonomic factors).
-- Notes:
--   - This is REFERENCE DATA, not regulatory text: no CFR hierarchy, no
--     requirement/severity-of-law semantics (contrast with OSHAStandard/
--     OSHARequirement, which model enforceable regulatory obligations).
--   - Global reference data: no TenantId column, no soft-delete columns
--     (deliberate — matches OSHAStandard/OSHARequirement).
--   - ProfileHash is the SHA-256 hex of the normalized profile text, used
--     for delta detection on re-ingestion (mirrors OSHAStandard.SectionTextHash).
--   - CASNumber is the natural upsert key when present; ChemicalName is the
--     fallback upsert key when CAS is null (not every NIOSH hazard entry has
--     a CAS Number, e.g. physical/ergonomic hazards).
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='Compliance') EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;')
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NIOSHHazardProfile' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NIOSHHazardProfile]
    (
        [HazardId]              UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NIOSHHazardProfile_HazardId DEFAULT NEWID(),
        [ChemicalName]          NVARCHAR(200)        NOT NULL,
        [CASNumber]             NVARCHAR(50)         NULL,
        [REL_TWA]               FLOAT                NULL,
        [REL_STEL]              FLOAT                NULL,
        [HealthEffects]         NVARCHAR(MAX)        NULL,
        [RecommendedControls]   NVARCHAR(MAX)        NULL,
        [PPERecommendations]    NVARCHAR(MAX)        NULL,
        [AtmosphericMonitoring] NVARCHAR(MAX)        NULL,
        [ErgonomicFactors]      NVARCHAR(MAX)        NULL,
        [SourceUrl]             NVARCHAR(500)        NULL,
        [ProfileHash]           CHAR(64)             NULL,
        [CreatedAtUtc]          DATETIME2(3)         NOT NULL CONSTRAINT DF_NIOSHHazardProfile_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        [UpdatedAtUtc]          DATETIME2(3)         NULL,
        [RowVersion]            ROWVERSION,

        CONSTRAINT PK_NIOSHHazardProfile PRIMARY KEY CLUSTERED ([HazardId])
    );
END
GO

------------------------------------------------------------
-- Unique upsert keys
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_NIOSHHazardProfile_CASNumber' AND object_id = OBJECT_ID('Compliance.NIOSHHazardProfile'))
    CREATE UNIQUE NONCLUSTERED INDEX UX_NIOSHHazardProfile_CASNumber
        ON [Compliance].[NIOSHHazardProfile] ([CASNumber])
        WHERE [CASNumber] IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_NIOSHHazardProfile_ChemicalName' AND object_id = OBJECT_ID('Compliance.NIOSHHazardProfile'))
    CREATE UNIQUE NONCLUSTERED INDEX UX_NIOSHHazardProfile_ChemicalName
        ON [Compliance].[NIOSHHazardProfile] ([ChemicalName]);
GO

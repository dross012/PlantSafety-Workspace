-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           NFPA_HazardProfile.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Global reference table holding fire/electrical/hot-work/flammable-materials
--                  hazard-guidance profiles keyed to an NFPA code number and section (e.g. NFPA 70
--                  National Electrical Code, NFPA 51B Hot Work). NFPA is NOT a government agency
--                  and publishes voluntary consensus standards, not legally binding regulations
--                  (contrast with Compliance.OSHAStandard/OSHARequirement, which model enforceable
--                  CFR Title 29 obligations imported verbatim from eCFR). Accordingly this table
--                  holds guidance/hazard-profile shapes -- hazard name/description, recommended
--                  controls, PPE/electrical-safety/hot-work/flammable-materials guidance -- never
--                  fabricated CFR-style hierarchical section text and never "requirement"/
--                  "severity-of-law" semantics. Not tenant-scoped -- this is shared master data,
--                  not tenant-owned business data (no TenantId, no soft-delete), matching the
--                  OSHAStandard/NIOSHHazardProfile precedent. Re-import is an upsert keyed on
--                  (CodeNumber, SectionNumber) so the pipeline can be re-run as the source is
--                  periodically synced. ModuleName mapping and cross-domain rule consumption
--                  happen through Compliance.NFPA_HazardModuleMapping and Fact Providers, not on
--                  this table. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NFPA_HazardProfile' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NFPA_HazardProfile] (
        [HazardId]                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NFPA_HazardProfile_HazardId DEFAULT (NEWID()) CONSTRAINT PK_NFPA_HazardProfile PRIMARY KEY,
        [CodeNumber]                 NVARCHAR(20)     NOT NULL,
        [SectionNumber]              NVARCHAR(50)     NOT NULL,
        [HazardName]                 NVARCHAR(200)    NOT NULL,
        [HazardDescription]          NVARCHAR(MAX)    NOT NULL,
        [RecommendedControls]        NVARCHAR(MAX)    NULL,
        [PPERecommendations]         NVARCHAR(MAX)    NULL,
        [ElectricalSafetyGuidance]   NVARCHAR(MAX)    NULL,
        [HotWorkGuidance]            NVARCHAR(MAX)    NULL,
        [FlammableMaterialsGuidance] NVARCHAR(MAX)    NULL,
        [SourceUrl]                  NVARCHAR(500)    NULL,
        [CreatedOn]                  DATETIME2(3)     NOT NULL CONSTRAINT DF_NFPA_HazardProfile_CreatedOn DEFAULT (SYSUTCDATETIME()),
        [SectionTextHash]            CHAR(64)         NULL,
        RowVersion                   ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_NFPA_HazardProfile_CodeNumber_SectionNumber] ON [Compliance].[NFPA_HazardProfile] ([CodeNumber], [SectionNumber]);
    CREATE NONCLUSTERED INDEX [IX_NFPA_HazardProfile_CodeNumber] ON [Compliance].[NFPA_HazardProfile] ([CodeNumber]);
END
GO

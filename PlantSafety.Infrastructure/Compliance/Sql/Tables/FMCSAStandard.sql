-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           FMCSAStandard.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Global regulatory reference table holding CFR Title 49 (FMCSA -- Federal
--                  Motor Carrier Safety Administration) Parts/Subparts/Sections, imported
--                  verbatim from the eCFR bulk-data XML (govinfo.gov). Same legal character
--                  as CFR Title 29 (OSHA) -- enforceable federal regulation, not voluntary
--                  guidance (unlike the separate NFPA effort). Not tenant-scoped -- this is
--                  shared master data, not tenant-owned business data (no TenantId, no
--                  soft-delete), matching Compliance.OSHAStandard's precedent. One row per
--                  CFR section. Re-import is an upsert keyed on (PartNumber, SectionNumber)
--                  so the pipeline can be re-run as the source is periodically synced
--                  (GovInfo API updater, added later). AppliesToModule mapping and
--                  cross-domain rule consumption happen through Compliance.FMCSARequirement
--                  and Fact Providers, not on this table.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FMCSAStandard' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[FMCSAStandard] (
        [StandardId]      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FMCSAStandard PRIMARY KEY CONSTRAINT DF_FMCSAStandard_StandardId DEFAULT (NEWID()),
        [PartNumber]      NVARCHAR(10)     NOT NULL,
        [SubpartLetter]   NVARCHAR(10)     NULL,
        [SectionNumber]   NVARCHAR(50)     NOT NULL,
        [SectionTitle]    NVARCHAR(300)    NULL,
        [SectionText]     NVARCHAR(MAX)    NOT NULL,
        [SectionTextHash] CHAR(64)         NULL,
        [SourceUrl]       NVARCHAR(500)    NULL,
        [CreatedOn]       DATETIME2(3)     NOT NULL CONSTRAINT DF_FMCSAStandard_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion        ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_FMCSAStandard_PartNumber_SectionNumber] ON [Compliance].[FMCSAStandard] ([PartNumber], [SectionNumber]);
    CREATE NONCLUSTERED INDEX [IX_FMCSAStandard_PartNumber] ON [Compliance].[FMCSAStandard] ([PartNumber]);
END
GO

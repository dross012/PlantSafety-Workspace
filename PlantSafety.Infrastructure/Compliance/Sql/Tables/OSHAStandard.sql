-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           OSHAStandard.sql
--  Created:        2026-08-06
--  SchemaVersion:  2026.1
--  Description:    Global regulatory reference table holding CFR Title 29 (OSHA)
--                  Parts/Subparts/Sections, imported verbatim from the eCFR bulk-data
--                  XML (govinfo.gov). Not tenant-scoped -- this is shared master data,
--                  not tenant-owned business data (no TenantId, no soft-delete), matching
--                  the one existing global reference table in this codebase
--                  (AnalyticsUnifiedModel.DimDate). One row per CFR section. Re-import is
--                  an upsert keyed on (PartNumber, Section) so the pipeline can be re-run
--                  as the source is periodically synced (GovInfo API updater, added later).
--                  AppliesToModule mapping and cross-domain rule consumption happen through
--                  Compliance.OSHARequirement and Fact Providers, not on this table.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OSHAStandard' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[OSHAStandard] (
        [OSHAStandardId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OSHAStandard PRIMARY KEY,
        [TitleNumber]    INT              NOT NULL,
        [PartNumber]     INT              NOT NULL,
        [PartTitle]      NVARCHAR(400)    NULL,
        [Subpart]        NVARCHAR(50)     NULL,
        [SubpartTitle]   NVARCHAR(400)    NULL,
        [Section]        NVARCHAR(50)     NOT NULL,
        [SectionTitle]   NVARCHAR(MAX)    NULL,
        [SectionText]    NVARCHAR(MAX)    NOT NULL,
        [EffectiveDate]  DATE             NULL,
        [Status]         NVARCHAR(50)     NOT NULL CONSTRAINT DF_OSHAStandard_Status DEFAULT ('Active'),
        [SourceUrl]      NVARCHAR(500)    NULL,
        [SectionTextHash] CHAR(64)        NULL,
        [CreatedAtUtc]   DATETIME2(3)     NOT NULL CONSTRAINT DF_OSHAStandard_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc]   DATETIME2(3)     NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_OSHAStandard_PartNumber_Section] ON [Compliance].[OSHAStandard] ([PartNumber], [Section]);
    CREATE NONCLUSTERED INDEX [IX_OSHAStandard_Status] ON [Compliance].[OSHAStandard] ([Status]);
END
GO

-- SectionTextHash (SHA-256 of SectionText) backs the scheduled updater's delta detection --
-- added after the original table so environments provisioned before the updater still upgrade cleanly.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[OSHAStandard]') AND name = 'SectionTextHash')
BEGIN
    ALTER TABLE [Compliance].[OSHAStandard] ADD [SectionTextHash] CHAR(64) NULL;
END
GO

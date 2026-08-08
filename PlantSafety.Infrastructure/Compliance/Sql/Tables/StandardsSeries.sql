-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure - SQL Scripts
--  File:           StandardsSeries.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Named standard/series rows within a StandardsFamily (e.g. ANSI B11,
--                  ANSI Z244.1, ASME B30, ASME BPVC, ISO 12100, ISO 13849). Global
--                  regulatory-adjacent reference data mirroring the OSHAStandard/
--                  OSHARequirement and EpaProgram precedent, not tenant business data (no
--                  TenantId, no soft-delete). ANSI/ASME/ISO are non-government consensus
--                  standards bodies whose standards are frequently incorporated by reference
--                  into OSHA/state regulations, but the standards text itself is copyrighted
--                  -- this table stores series-level metadata only, not verbatim copyrighted
--                  standard text. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StandardsSeries' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[StandardsSeries] (
        [SeriesId]     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StandardsSeries PRIMARY KEY
                                            CONSTRAINT DF_StandardsSeries_SeriesId DEFAULT (NEWID()),
        [FamilyCode]   NVARCHAR(10)     NOT NULL CONSTRAINT FK_StandardsSeries_StandardsFamily REFERENCES [Compliance].[StandardsFamily] ([FamilyCode]),
        [SeriesCode]   NVARCHAR(30)     NOT NULL,
        [SeriesName]   NVARCHAR(300)    NOT NULL,
        [Description]  NVARCHAR(MAX)    NULL,
        [CreatedOn]    DATETIME2(3)     NOT NULL CONSTRAINT DF_StandardsSeries_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion     ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_StandardsSeries_FamilyCode_SeriesCode] ON [Compliance].[StandardsSeries] ([FamilyCode], [SeriesCode]);
    CREATE NONCLUSTERED INDEX [IX_StandardsSeries_FamilyCode] ON [Compliance].[StandardsSeries] ([FamilyCode]);
END
GO

-- Additive-only evolution.

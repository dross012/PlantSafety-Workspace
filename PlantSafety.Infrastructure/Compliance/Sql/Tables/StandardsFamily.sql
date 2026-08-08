-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure - SQL Scripts
--  File:           StandardsFamily.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Small static lookup of consensus-standards bodies (ANSI, ASME, ISO)
--                  whose standards are frequently incorporated by reference into OSHA/state
--                  regulations. Global regulatory-adjacent reference data mirroring the
--                  OSHAStandard/OSHARequirement and EpaProgram precedent, not tenant
--                  business data (no TenantId, no soft-delete). ANSI/ASME/ISO are
--                  non-government consensus standards bodies -- their standards text itself
--                  is copyrighted, so this table (and the rest of the Standards module)
--                  stores metadata/technical summaries only, never verbatim copyrighted
--                  standard text. Uses the "Standards" prefix on every table/constraint name
--                  (StandardsFamily, StandardsSeries, StandardsRequirement, etc.) rather than
--                  a bare "Standards" table, staying consistent with the discipline already
--                  applied to the EPA module's "Epa" prefix. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StandardsFamily' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[StandardsFamily] (
        [FamilyId]     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StandardsFamily PRIMARY KEY
                                            CONSTRAINT DF_StandardsFamily_FamilyId DEFAULT (NEWID()),
        [FamilyCode]   NVARCHAR(10)     NOT NULL,
        [FamilyName]   NVARCHAR(200)    NOT NULL,
        [Description]  NVARCHAR(MAX)    NULL,
        [CreatedOn]    DATETIME2(3)     NOT NULL CONSTRAINT DF_StandardsFamily_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion     ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_StandardsFamily_FamilyCode] ON [Compliance].[StandardsFamily] ([FamilyCode]);
END
GO

-- Additive-only evolution.

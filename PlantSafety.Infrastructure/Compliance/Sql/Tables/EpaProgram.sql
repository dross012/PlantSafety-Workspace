-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           EpaProgram.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Global regulatory reference table listing the EPA statutory programs
--                  (RCRA, TSCA, CERCLA/SARA, CAA, CWA, etc.) that PlantSafety's EPA
--                  reference-data tables key against. Mostly-static lookup data, imported
--                  from the governing statutes -- not tenant-owned business data (no
--                  TenantId, no soft-delete), matching the OSHAStandard/OSHARequirement
--                  precedent. All table/column names in this module use the "Epa" prefix
--                  (not bare "EPA") to avoid collision with the pre-existing, unrelated
--                  Compliance.Certification.EPA (CertificationEPA) and
--                  Compliance.RegulatoryCalendar.Deadlines.EPA (DeadlinesEPA) features,
--                  which are generic permit/deadline tracking tables and are not touched
--                  by this build. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EpaProgram' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[EpaProgram] (
        [ProgramId]                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EpaProgram PRIMARY KEY
                                                       CONSTRAINT DF_EpaProgram_ProgramId DEFAULT (NEWID()),
        [ProgramCode]              NVARCHAR(20)     NOT NULL,
        [ProgramName]              NVARCHAR(200)    NOT NULL,
        [Description]              NVARCHAR(MAX)    NULL,
        [PrimaryStatuteCitation]   NVARCHAR(100)    NULL,
        [CreatedOn]                DATETIME2(3)     NOT NULL CONSTRAINT DF_EpaProgram_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                 ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_EpaProgram_ProgramCode] ON [Compliance].[EpaProgram] ([ProgramCode]);
END
GO

-- Additive-only evolution.

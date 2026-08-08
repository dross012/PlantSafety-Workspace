-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           EpaRecordkeepingRequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    EPA-mandated recordkeeping/retention requirements tied to an EPA
--                  program and optionally to a single EpaRegulatoryRequirement. Global
--                  regulatory reference data mirroring the OSHAStandard/OSHARequirement
--                  precedent, not tenant business data (no TenantId, no soft-delete).
--                  RequiredDataElements is a list-valued field with no child-table
--                  precedent for this class of table (matching the recent PHMSA build's
--                  guidance for the same situation), so it is stored as a single
--                  NVARCHAR(MAX) column holding a native SQL Server JSON array of strings,
--                  validated with ISJSON, defaulting to an empty array. Uses the "Epa"
--                  prefix (not bare "EPA") to avoid collision with the pre-existing,
--                  unrelated Compliance.Certification.EPA / Compliance.RegulatoryCalendar.
--                  Deadlines.EPA features (CertificationEPA / DeadlinesEPA tables), which
--                  are not touched by this build. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EpaRecordkeepingRequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[EpaRecordkeepingRequirement] (
        [RecordkeepingRequirementId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EpaRecordkeepingRequirement PRIMARY KEY
                                                          CONSTRAINT DF_EpaRecordkeepingRequirement_RecordkeepingRequirementId DEFAULT (NEWID()),
        [ProgramCode]                 NVARCHAR(20)     NOT NULL CONSTRAINT FK_EpaRecordkeepingRequirement_EpaProgram REFERENCES [Compliance].[EpaProgram] ([ProgramCode]),
        [RequirementId]               UNIQUEIDENTIFIER NULL     CONSTRAINT FK_EpaRecordkeepingRequirement_EpaRegulatoryRequirement REFERENCES [Compliance].[EpaRegulatoryRequirement] ([RequirementId]),
        [RetentionPeriodYears]        INT              NOT NULL,
        [RequiredDataElements]        NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_EpaRecordkeepingRequirement_RequiredDataElements DEFAULT (N'[]'),
        [Description]                 NVARCHAR(MAX)    NULL,
        [CreatedOn]                   DATETIME2(3)     NOT NULL CONSTRAINT DF_EpaRecordkeepingRequirement_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                    ROWVERSION       NOT NULL,
        CONSTRAINT CK_EpaRecordkeepingRequirement_RequiredDataElements_Json CHECK (ISJSON([RequiredDataElements]) = 1)
    );

    CREATE NONCLUSTERED INDEX [IX_EpaRecordkeepingRequirement_ProgramCode] ON [Compliance].[EpaRecordkeepingRequirement] ([ProgramCode]);
    CREATE NONCLUSTERED INDEX [IX_EpaRecordkeepingRequirement_RequirementId] ON [Compliance].[EpaRecordkeepingRequirement] ([RequirementId]);
END
GO

-- Additive-only evolution.

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           FMCSAStandardChangeLog.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Audit trail written by a scheduled FMCSA updater (mirroring
--                  tools/OshaStandardsImport) every time it detects a New, Revised, or
--                  Rescinded CFR Title 49 section (SHA-256 content-hash diff against
--                  Compliance.FMCSAStandard.SectionTextHash). This is what lets PlantSafety
--                  surface "regulation changed" alerts and answer "show me sections updated
--                  since <date>" without re-diffing text. Global reference data, same
--                  scoping rules as FMCSAStandard (no TenantId).
--                  Append-only: rows are inserted by the updater's change-detection pass and
--                  never subsequently UPDATEd or DELETEd -- this table is the immutable
--                  history of every detected change, not a mutable current-state record
--                  (current state lives on Compliance.FMCSAStandard itself).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FMCSAStandardChangeLog' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[FMCSAStandardChangeLog] (
        [ChangeLogId]      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FMCSAStandardChangeLog PRIMARY KEY CONSTRAINT DF_FMCSAStandardChangeLog_ChangeLogId DEFAULT (NEWID()),
        [StandardId]       UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FMCSAStandardChangeLog_FMCSAStandard REFERENCES [Compliance].[FMCSAStandard] ([StandardId]),
        [PartNumber]       NVARCHAR(10)     NOT NULL,
        [SectionNumber]    NVARCHAR(50)     NOT NULL,
        [ChangeType]       NVARCHAR(20)     NOT NULL,
        [PreviousTextHash] CHAR(64)         NULL,
        [NewTextHash]      CHAR(64)         NULL,
        [DetectedAtUtc]    DATETIME2(3)     NOT NULL CONSTRAINT DF_FMCSAStandardChangeLog_DetectedAtUtc DEFAULT (SYSUTCDATETIME()),
        [Summary]          NVARCHAR(MAX)    NULL,
        CONSTRAINT CK_FMCSAStandardChangeLog_ChangeType CHECK ([ChangeType] IN ('New', 'Revised', 'Rescinded'))
    );

    CREATE NONCLUSTERED INDEX [IX_FMCSAStandardChangeLog_DetectedAtUtc] ON [Compliance].[FMCSAStandardChangeLog] ([DetectedAtUtc]);
    CREATE NONCLUSTERED INDEX [IX_FMCSAStandardChangeLog_StandardId] ON [Compliance].[FMCSAStandardChangeLog] ([StandardId]);
END
GO

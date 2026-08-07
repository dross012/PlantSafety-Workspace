-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           OSHAStandardChangeLog.sql
--  Created:        2026-08-06
--  SchemaVersion:  2026.1
--  Description:    Audit trail written by the scheduled OSHA updater (tools/OshaStandardsImport)
--                  every time it detects a New, Revised, or Rescinded CFR Title 29 section
--                  (SHA-256 content-hash diff against Compliance.OSHAStandard.SectionTextHash).
--                  This is what lets PlantSafety surface "regulation changed" alerts and answer
--                  "show me sections updated since <date>" without re-diffing text. Global
--                  reference data, same scoping rules as OSHAStandard (no TenantId).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OSHAStandardChangeLog' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[OSHAStandardChangeLog] (
        [ChangeLogId]      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OSHAStandardChangeLog PRIMARY KEY,
        [OSHAStandardId]   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_OSHAStandardChangeLog_OSHAStandard REFERENCES [Compliance].[OSHAStandard] ([OSHAStandardId]),
        [PartNumber]       INT              NOT NULL,
        [Section]          NVARCHAR(50)     NOT NULL,
        [ChangeType]       NVARCHAR(20)     NOT NULL,
        [PreviousStatus]   NVARCHAR(50)     NULL,
        [NewStatus]        NVARCHAR(50)     NULL,
        [PreviousTextHash] CHAR(64)         NULL,
        [NewTextHash]      CHAR(64)         NULL,
        [DetectedAtUtc]    DATETIME2(3)     NOT NULL CONSTRAINT DF_OSHAStandardChangeLog_DetectedAtUtc DEFAULT (SYSUTCDATETIME()),
        [Summary]          NVARCHAR(500)    NULL,
        CONSTRAINT CK_OSHAStandardChangeLog_ChangeType CHECK ([ChangeType] IN ('New', 'Revised', 'Rescinded'))
    );

    CREATE NONCLUSTERED INDEX [IX_OSHAStandardChangeLog_DetectedAtUtc] ON [Compliance].[OSHAStandardChangeLog] ([DetectedAtUtc]);
    CREATE NONCLUSTERED INDEX [IX_OSHAStandardChangeLog_OSHAStandardId] ON [Compliance].[OSHAStandardChangeLog] ([OSHAStandardId]);
END
GO

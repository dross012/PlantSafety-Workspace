-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           NFPA_HazardChangeLog.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Audit trail written whenever a New, Revised, or Rescinded NFPA hazard-profile
--                  entry is detected on re-ingestion (SHA-256 content-hash diff against
--                  Compliance.NFPA_HazardProfile.SectionTextHash). Mirrors
--                  Compliance.OSHAStandardChangeLog. Lets PlantSafety surface "guidance changed"
--                  alerts and answer "show me hazard profiles updated since <date>" without
--                  re-diffing text. Global reference data, same scoping rules as
--                  NFPA_HazardProfile (no TenantId). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NFPA_HazardChangeLog' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NFPA_HazardChangeLog] (
        [ChangeLogId]      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NFPA_HazardChangeLog_ChangeLogId DEFAULT (NEWID()) CONSTRAINT PK_NFPA_HazardChangeLog PRIMARY KEY,
        [HazardId]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_NFPA_HazardChangeLog_NFPA_HazardProfile REFERENCES [Compliance].[NFPA_HazardProfile] ([HazardId]),
        [CodeNumber]       NVARCHAR(20)     NOT NULL,
        [SectionNumber]    NVARCHAR(50)     NOT NULL,
        [ChangeType]       NVARCHAR(20)     NOT NULL,
        [PreviousTextHash] CHAR(64)         NULL,
        [NewTextHash]      CHAR(64)         NULL,
        [DetectedAtUtc]    DATETIME2(3)     NOT NULL CONSTRAINT DF_NFPA_HazardChangeLog_DetectedAtUtc DEFAULT (SYSUTCDATETIME()),
        [Summary]          NVARCHAR(MAX)    NULL,
        CONSTRAINT CK_NFPA_HazardChangeLog_ChangeType CHECK ([ChangeType] IN ('New', 'Revised', 'Rescinded'))
    );

    CREATE NONCLUSTERED INDEX [IX_NFPA_HazardChangeLog_DetectedAtUtc] ON [Compliance].[NFPA_HazardChangeLog] ([DetectedAtUtc]);
    CREATE NONCLUSTERED INDEX [IX_NFPA_HazardChangeLog_HazardId] ON [Compliance].[NFPA_HazardChangeLog] ([HazardId]);
END
GO

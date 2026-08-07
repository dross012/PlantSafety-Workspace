USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Table:         Compliance.NIOSHHazardChangeLog
-- Description:   Append-only delta/audit table recording New/Revised/Removed
--                changes detected on NIOSHHazardProfile re-ingestion, driven
--                by ProfileHash comparison (mirrors OSHAStandardChangeLog).
-- Notes:
--   - Append-only: inserts only, no updates/deletes (validate-append-only).
--   - No RowVersion — this table is never updated in place.
--   - No soft-delete columns, no TenantId (global reference data).
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='Compliance') EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;')
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NIOSHHazardChangeLog' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NIOSHHazardChangeLog]
    (
        [ChangeLogId]           UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NIOSHHazardChangeLog_ChangeLogId DEFAULT NEWID(),
        [HazardId]              UNIQUEIDENTIFIER    NOT NULL,
        [ChemicalName]          NVARCHAR(200)       NOT NULL,
        [ChangeType]            NVARCHAR(20)        NOT NULL,
        [PreviousProfileHash]   CHAR(64)            NULL,
        [NewProfileHash]        CHAR(64)            NULL,
        [DetectedAtUtc]         DATETIME2(3)        NOT NULL CONSTRAINT DF_NIOSHHazardChangeLog_DetectedAtUtc DEFAULT SYSUTCDATETIME(),
        [Summary]               NVARCHAR(500)       NULL,

        CONSTRAINT PK_NIOSHHazardChangeLog PRIMARY KEY CLUSTERED ([ChangeLogId]),
        CONSTRAINT FK_NIOSHHazardChangeLog_NIOSHHazardProfile FOREIGN KEY ([HazardId])
            REFERENCES [Compliance].[NIOSHHazardProfile] ([HazardId]),
        CONSTRAINT CK_NIOSHHazardChangeLog_ChangeType CHECK ([ChangeType] IN ('New','Revised','Removed'))
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHHazardChangeLog_DetectedAtUtc' AND object_id = OBJECT_ID('Compliance.NIOSHHazardChangeLog'))
    CREATE NONCLUSTERED INDEX IX_NIOSHHazardChangeLog_DetectedAtUtc
        ON [Compliance].[NIOSHHazardChangeLog] ([DetectedAtUtc]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHHazardChangeLog_HazardId' AND object_id = OBJECT_ID('Compliance.NIOSHHazardChangeLog'))
    CREATE NONCLUSTERED INDEX IX_NIOSHHazardChangeLog_HazardId
        ON [Compliance].[NIOSHHazardChangeLog] ([HazardId]);
GO

------------------------------------------------------------
-- Append-only enforcement: block UPDATE/DELETE (validate-append-only)
------------------------------------------------------------
IF OBJECT_ID('Compliance.trg_NIOSHHazardChangeLog_NoUpdateDelete', 'TR') IS NOT NULL
    DROP TRIGGER [Compliance].[trg_NIOSHHazardChangeLog_NoUpdateDelete];
GO

CREATE TRIGGER [Compliance].[trg_NIOSHHazardChangeLog_NoUpdateDelete]
ON [Compliance].[NIOSHHazardChangeLog]
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50010, 'Compliance.NIOSHHazardChangeLog is append-only; UPDATE/DELETE are not permitted.', 1;
END
GO

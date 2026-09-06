-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:  50-governance.sql
-- Purpose: VersionManifest, DriftCheck, and retention/archival for the IncidentMgtIntegration
--          schema, mirroring EmergencyMgt-Integration's own 50-governance.sql pattern exactly,
--          scoped to this integration schema's own tables (IncidentSummaryFeed and
--          GovernanceConsumptionLog -- this schema has no separate lifecycle/corrective-action/
--          readiness tables the way EmergencyMgt-Integration does; IncidentSummaryFeed already
--          carries investigation/corrective/regulatory-compliance status flattened into one
--          row per incident).
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ------------------------------------------------------------------
-- VersionManifest -- one row per computed snapshot of this schema's deployed object state.
-- Append-only (TR_IncidentMgtIntegration_VersionManifest_NoDelete guards it).
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'VersionManifest')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[VersionManifest] (
        [VersionManifestId]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_VersionManifest_Id DEFAULT NEWID(),
        [SchemaVersionLabel]   NVARCHAR(100) NOT NULL,
        [ManifestJson]         NVARCHAR(MAX) NOT NULL,
        [ManifestHash]         VARBINARY(32) NOT NULL,
        [RsaSignature]         VARBINARY(512) NULL,
        [SignedByThumbprint]   NVARCHAR(64) NULL,
        [CreatedUtc]           DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_VersionManifest_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_IncidentMgtIntegration_VersionManifest PRIMARY KEY CLUSTERED ([VersionManifestId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_IncidentMgtIntegration_VersionManifest_NoDelete')
BEGIN
    EXEC('
    CREATE TRIGGER [IncidentMgtIntegration].[TR_IncidentMgtIntegration_VersionManifest_NoDelete]
    ON [IncidentMgtIntegration].[VersionManifest]
    INSTEAD OF DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        RAISERROR(N''IncidentMgtIntegration.VersionManifest is append-only; rows cannot be deleted.'', 16, 1);
    END');
END
GO

-- ------------------------------------------------------------------
-- DriftCheck -- recordable, resolvable drift findings for this schema (log-table mechanism,
-- matching EmergencyMgt-Integration's own Record/GetUnresolved/Resolve convention).
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'DriftCheck')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[DriftCheck] (
        [DriftCheckId]   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IncidentMgtIntegration_DriftCheck_Id DEFAULT NEWID(),
        [ObjectName]     NVARCHAR(256) NOT NULL,
        [DriftType]      NVARCHAR(30) NOT NULL,  -- Schema | RoutingKey | DataShape
        [Detail]         NVARCHAR(400) NOT NULL,
        [DetectedUtc]    DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_DriftCheck_DetectedUtc DEFAULT SYSUTCDATETIME(),
        [IsResolved]     BIT NOT NULL CONSTRAINT DF_IncidentMgtIntegration_DriftCheck_IsResolved DEFAULT 0,
        [ResolvedUtc]    DATETIME2 NULL,
        CONSTRAINT PK_IncidentMgtIntegration_DriftCheck PRIMARY KEY CLUSTERED ([DriftCheckId]),
        CONSTRAINT CK_IncidentMgtIntegration_DriftCheck_DriftType CHECK ([DriftType] IN (N'Schema', N'RoutingKey', N'DataShape'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_IncidentMgtIntegration_DriftCheck_NoDelete')
BEGIN
    EXEC('
    CREATE TRIGGER [IncidentMgtIntegration].[TR_IncidentMgtIntegration_DriftCheck_NoDelete]
    ON [IncidentMgtIntegration].[DriftCheck]
    INSTEAD OF DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        RAISERROR(N''IncidentMgtIntegration.DriftCheck is append-only; rows cannot be deleted (resolve, do not delete).'', 16, 1);
    END');
END
GO

-- ------------------------------------------------------------------
-- ArchiveRegistry + retention enforcement for IncidentSummaryFeed and GovernanceConsumptionLog
-- (rows past a 3-year cutoff on ReceivedUtc/ConsumedUtc), mirroring EmergencyMgt-Integration's
-- own ArchiveRegistry/usp_EnforceRetention trio.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'IncidentMgtIntegration' AND t.name = 'ArchiveRegistry')
BEGIN
    CREATE TABLE [IncidentMgtIntegration].[ArchiveRegistry] (
        [ArchiveRegistryId] BIGINT IDENTITY(1,1) NOT NULL,
        [SourceTable]       NVARCHAR(128) NOT NULL,
        [SourceRowId]       UNIQUEIDENTIFIER NOT NULL,
        [RowSnapshotJson]   NVARCHAR(MAX) NOT NULL,
        [ArchivedUtc]       DATETIME2 NOT NULL CONSTRAINT DF_IncidentMgtIntegration_ArchiveRegistry_ArchivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_IncidentMgtIntegration_ArchiveRegistry PRIMARY KEY CLUSTERED ([ArchiveRegistryId])
    );
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_VersionManifest_ComputeAndRecord]
    @SchemaVersionLabel NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @ManifestJson NVARCHAR(MAX);

        SELECT @ManifestJson = (
            SELECT o.name AS ObjectName, o.type_desc AS ObjectType, o.modify_date AS ModifyDate
            FROM sys.objects o
            JOIN sys.schemas s ON o.schema_id = s.schema_id
            WHERE s.name = N'IncidentMgtIntegration'
              AND o.type IN ('U', 'V', 'P', 'TR')
            ORDER BY o.type_desc, o.name
            FOR JSON PATH
        );

        DECLARE @Hash VARBINARY(32) = HASHBYTES('SHA2_256', @ManifestJson);

        DECLARE @Id UNIQUEIDENTIFIER = NEWID();
        INSERT INTO [IncidentMgtIntegration].[VersionManifest] ([VersionManifestId], [SchemaVersionLabel], [ManifestJson], [ManifestHash])
        VALUES (@Id, @SchemaVersionLabel, @ManifestJson, @Hash);

        SELECT VersionManifestId, SchemaVersionLabel, ManifestHash, CreatedUtc
        FROM [IncidentMgtIntegration].[VersionManifest]
        WHERE VersionManifestId = @Id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_VersionManifest_SetSignature]
    @VersionManifestId UNIQUEIDENTIFIER,
    @RsaSignature       VARBINARY(512),
    @SignedByThumbprint NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        UPDATE [IncidentMgtIntegration].[VersionManifest]
        SET [RsaSignature] = @RsaSignature, [SignedByThumbprint] = @SignedByThumbprint
        WHERE [VersionManifestId] = @VersionManifestId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_VersionManifest_GetLatest]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 VersionManifestId, SchemaVersionLabel, ManifestJson, ManifestHash, RsaSignature, SignedByThumbprint, CreatedUtc
    FROM [IncidentMgtIntegration].[VersionManifest]
    ORDER BY CreatedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_DriftCheck_Record]
    @ObjectName NVARCHAR(256),
    @DriftType  NVARCHAR(30),
    @Detail     NVARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        INSERT INTO [IncidentMgtIntegration].[DriftCheck] ([ObjectName], [DriftType], [Detail])
        VALUES (@ObjectName, @DriftType, @Detail);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_DriftCheck_GetUnresolved]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DriftCheckId, ObjectName, DriftType, Detail, DetectedUtc
    FROM [IncidentMgtIntegration].[DriftCheck]
    WHERE IsResolved = 0
    ORDER BY DetectedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_DriftCheck_Resolve]
    @DriftCheckId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        UPDATE [IncidentMgtIntegration].[DriftCheck]
        SET [IsResolved] = 1, [ResolvedUtc] = SYSUTCDATETIME()
        WHERE [DriftCheckId] = @DriftCheckId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Retention: rows older than 3 years are archived (row snapshot copied to ArchiveRegistry)
-- then deleted from the live table. Fixed allow-list, never dynamic SQL, matching
-- EmergencyMgt-Integration's own usp_EnforceRetention precedent.
CREATE OR ALTER PROCEDURE [IncidentMgtIntegration].[usp_EnforceRetention]
    @RetentionYears INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @CutoffUtc DATETIME2 = DATEADD(YEAR, -@RetentionYears, SYSUTCDATETIME());
        DECLARE @TotalArchived INT = 0;

        BEGIN TRANSACTION;

        INSERT INTO [IncidentMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'IncidentSummaryFeed', IncidentSummaryFeedId,
               (SELECT * FROM [IncidentMgtIntegration].[IncidentSummaryFeed] x WHERE x.IncidentSummaryFeedId = f.IncidentSummaryFeedId FOR JSON AUTO)
        FROM [IncidentMgtIntegration].[IncidentSummaryFeed] f
        WHERE f.ReceivedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [IncidentMgtIntegration].[IncidentSummaryFeed] WHERE ReceivedUtc < @CutoffUtc;

        INSERT INTO [IncidentMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'GovernanceConsumptionLog', GovernanceConsumptionLogId,
               (SELECT * FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] x WHERE x.GovernanceConsumptionLogId = g.GovernanceConsumptionLogId FOR JSON AUTO)
        FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] g
        WHERE g.ConsumedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [IncidentMgtIntegration].[GovernanceConsumptionLog] WHERE ConsumedUtc < @CutoffUtc;

        COMMIT TRANSACTION;

        SELECT @TotalArchived AS TotalArchived, 0 AS ResultCode, N'Success' AS ResultMessage;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

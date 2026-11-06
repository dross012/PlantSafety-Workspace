-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:  50-governance.sql
-- Purpose: VersionManifest, DriftCheck, and retention/archival for the EmergencyMgtIntegration
--          schema, mirroring EmergencyMgt's own Governance.VersionManifest/DriftCheck/
--          ArchiveRegistry pattern (source/database/EmergencyManagementDB/Governance in the
--          EmergencyMgt repo) exactly, scoped to this integration schema.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ------------------------------------------------------------------
-- VersionManifest -- one row per computed snapshot of this schema's deployed object state.
-- Append-only (TR_EmergencyMgtIntegration_VersionManifest_NoDelete guards it).
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'VersionManifest')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[VersionManifest] (
        [VersionManifestId]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_VersionManifest_Id DEFAULT NEWID(),
        [SchemaVersionLabel]   NVARCHAR(100) NOT NULL,
        [ManifestJson]         NVARCHAR(MAX) NOT NULL,
        [ManifestHash]         VARBINARY(32) NOT NULL,
        [RsaSignature]         VARBINARY(512) NULL,
        [SignedByThumbprint]   NVARCHAR(64) NULL,
        [CreatedUtc]           DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_VersionManifest_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_VersionManifest PRIMARY KEY CLUSTERED ([VersionManifestId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_EmergencyMgtIntegration_VersionManifest_NoDelete')
BEGIN
    EXEC('
    CREATE TRIGGER [EmergencyMgtIntegration].[TR_EmergencyMgtIntegration_VersionManifest_NoDelete]
    ON [EmergencyMgtIntegration].[VersionManifest]
    INSTEAD OF DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        RAISERROR(N''EmergencyMgtIntegration.VersionManifest is append-only; rows cannot be deleted.'', 16, 1);
    END');
END
GO

-- ------------------------------------------------------------------
-- DriftCheck -- recordable, resolvable drift findings for this schema (log-table mechanism,
-- matching the "drift-safe" precedent both EmergencyMgt's own Governance.DriftCheck and
-- PredictiveSafety's RiskDriftCheck already establish program-wide: Record/GetUnresolved/
-- Resolve, no live sys.* comparison engine).
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'DriftCheck')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[DriftCheck] (
        [DriftCheckId]   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_DriftCheck_Id DEFAULT NEWID(),
        [ObjectName]     NVARCHAR(256) NOT NULL,
        [DriftType]      NVARCHAR(30) NOT NULL,  -- Schema | RoutingKey | DataShape
        [Detail]         NVARCHAR(400) NOT NULL,
        [DetectedUtc]    DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_DriftCheck_DetectedUtc DEFAULT SYSUTCDATETIME(),
        [IsResolved]     BIT NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_DriftCheck_IsResolved DEFAULT 0,
        [ResolvedUtc]    DATETIME2 NULL,
        CONSTRAINT PK_EmergencyMgtIntegration_DriftCheck PRIMARY KEY CLUSTERED ([DriftCheckId]),
        CONSTRAINT CK_EmergencyMgtIntegration_DriftCheck_DriftType CHECK ([DriftType] IN (N'Schema', N'RoutingKey', N'DataShape'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_EmergencyMgtIntegration_DriftCheck_NoDelete')
BEGIN
    EXEC('
    CREATE TRIGGER [EmergencyMgtIntegration].[TR_EmergencyMgtIntegration_DriftCheck_NoDelete]
    ON [EmergencyMgtIntegration].[DriftCheck]
    INSTEAD OF DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        RAISERROR(N''EmergencyMgtIntegration.DriftCheck is append-only; rows cannot be deleted (resolve, do not delete).'', 16, 1);
    END');
END
GO

-- ------------------------------------------------------------------
-- ArchiveRegistry + retention enforcement for the 4 inbound feed tables (append-only *Inbound
-- rows past a 3-year cutoff on ReceivedUtc, mirroring EmergencyMgt's own Governance.
-- ArchiveRegistry/usp_Governance_EnforceRetention trio).
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'EmergencyMgtIntegration' AND t.name = 'ArchiveRegistry')
BEGIN
    CREATE TABLE [EmergencyMgtIntegration].[ArchiveRegistry] (
        [ArchiveRegistryId] BIGINT IDENTITY(1,1) NOT NULL,
        [SourceTable]       NVARCHAR(128) NOT NULL,
        [SourceRowId]       UNIQUEIDENTIFIER NOT NULL,
        [RowSnapshotJson]   NVARCHAR(MAX) NOT NULL,
        [ArchivedUtc]       DATETIME2 NOT NULL CONSTRAINT DF_EmergencyMgtIntegration_ArchiveRegistry_ArchivedUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EmergencyMgtIntegration_ArchiveRegistry PRIMARY KEY CLUSTERED ([ArchiveRegistryId])
    );
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_VersionManifest_ComputeAndRecord]
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
            WHERE s.name = N'EmergencyMgtIntegration'
              AND o.type IN ('U', 'V', 'P', 'TR')
            ORDER BY o.type_desc, o.name
            FOR JSON PATH
        );

        DECLARE @Hash VARBINARY(32) = HASHBYTES('SHA2_256', @ManifestJson);

        DECLARE @Id UNIQUEIDENTIFIER = NEWID();
        INSERT INTO [EmergencyMgtIntegration].[VersionManifest] ([VersionManifestId], [SchemaVersionLabel], [ManifestJson], [ManifestHash])
        VALUES (@Id, @SchemaVersionLabel, @ManifestJson, @Hash);

        SELECT VersionManifestId, SchemaVersionLabel, ManifestHash, CreatedUtc
        FROM [EmergencyMgtIntegration].[VersionManifest]
        WHERE VersionManifestId = @Id;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_VersionManifest_SetSignature]
    @VersionManifestId UNIQUEIDENTIFIER,
    @RsaSignature       VARBINARY(512),
    @SignedByThumbprint NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        UPDATE [EmergencyMgtIntegration].[VersionManifest]
        SET [RsaSignature] = @RsaSignature, [SignedByThumbprint] = @SignedByThumbprint
        WHERE [VersionManifestId] = @VersionManifestId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_VersionManifest_GetLatest]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 VersionManifestId, SchemaVersionLabel, ManifestJson, ManifestHash, RsaSignature, SignedByThumbprint, CreatedUtc
    FROM [EmergencyMgtIntegration].[VersionManifest]
    ORDER BY CreatedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_DriftCheck_Record]
    @ObjectName NVARCHAR(256),
    @DriftType  NVARCHAR(30),
    @Detail     NVARCHAR(400)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        INSERT INTO [EmergencyMgtIntegration].[DriftCheck] ([ObjectName], [DriftType], [Detail])
        VALUES (@ObjectName, @DriftType, @Detail);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_DriftCheck_GetUnresolved]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DriftCheckId, ObjectName, DriftType, Detail, DetectedUtc
    FROM [EmergencyMgtIntegration].[DriftCheck]
    WHERE IsResolved = 0
    ORDER BY DetectedUtc DESC;
END
GO

CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_DriftCheck_Resolve]
    @DriftCheckId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        UPDATE [EmergencyMgtIntegration].[DriftCheck]
        SET [IsResolved] = 1, [ResolvedUtc] = SYSUTCDATETIME()
        WHERE [DriftCheckId] = @DriftCheckId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- Retention: rows older than 3 years (ReceivedUtc) in the 4 inbound feed tables are archived
-- (row snapshot copied to ArchiveRegistry) then deleted from the live table. Fixed allow-list,
-- never dynamic SQL, matching EmergencyMgt's own usp_Governance_ArchiveRows precedent.
CREATE OR ALTER PROCEDURE [EmergencyMgtIntegration].[usp_EnforceRetention]
    @RetentionYears INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @CutoffUtc DATETIME2 = DATEADD(YEAR, -@RetentionYears, SYSUTCDATETIME());
        DECLARE @TotalArchived INT = 0;

        BEGIN TRANSACTION;

        INSERT INTO [EmergencyMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'EmergencySummaryInbound', EmergencySummaryInboundId,
               (SELECT * FROM [EmergencyMgtIntegration].[EmergencySummaryInbound] x WHERE x.EmergencySummaryInboundId = s.EmergencySummaryInboundId FOR JSON AUTO)
        FROM [EmergencyMgtIntegration].[EmergencySummaryInbound] s
        WHERE s.ReceivedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [EmergencyMgtIntegration].[EmergencySummaryInbound] WHERE ReceivedUtc < @CutoffUtc;

        INSERT INTO [EmergencyMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'EmergencyLifecycleInbound', EmergencyLifecycleInboundId,
               (SELECT * FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound] x WHERE x.EmergencyLifecycleInboundId = l.EmergencyLifecycleInboundId FOR JSON AUTO)
        FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound] l
        WHERE l.ReceivedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound] WHERE ReceivedUtc < @CutoffUtc;

        INSERT INTO [EmergencyMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'EmergencyCorrectiveActionInbound', EmergencyCorrectiveActionInboundId,
               (SELECT * FROM [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] x WHERE x.EmergencyCorrectiveActionInboundId = c.EmergencyCorrectiveActionInboundId FOR JSON AUTO)
        FROM [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] c
        WHERE c.ReceivedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound] WHERE ReceivedUtc < @CutoffUtc;

        INSERT INTO [EmergencyMgtIntegration].[ArchiveRegistry] ([SourceTable], [SourceRowId], [RowSnapshotJson])
        SELECT N'EmergencyReadinessInbound', EmergencyReadinessInboundId,
               (SELECT * FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound] x WHERE x.EmergencyReadinessInboundId = r.EmergencyReadinessInboundId FOR JSON AUTO)
        FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound] r
        WHERE r.ReceivedUtc < @CutoffUtc;
        SET @TotalArchived += @@ROWCOUNT;

        DELETE FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound] WHERE ReceivedUtc < @CutoffUtc;

        COMMIT TRANSACTION;

        SELECT @TotalArchived AS TotalArchived, 0 AS ResultCode, N'Success' AS ResultMessage;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

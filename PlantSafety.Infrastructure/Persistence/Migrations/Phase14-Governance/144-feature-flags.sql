-- =============================================
-- Migration:     144-feature-flags.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   DB-level ops-controlled feature flags.
--
-- Genuine-gap check performed first (per task instructions): searched
-- PlantSafety.Application/PlantSafety.Infrastructure/PlantSafety.Api for an
-- existing IFeatureFlag/IFeatureManager-style .NET abstraction. Found only a
-- single TODO comment (PlantSafety.Api/Extensions/ServiceCollectionExtensions.cs
-- line 29: "Add per-module feature flags (IFeatureManager) for gradual
-- rollout") -- no implementation exists. There is also a UI-facing
-- TenantDto.FeatureFlags (PlantSafety.UI/Models/Auth/TenantDto.cs), but that
-- is tenant/subscription entitlement data served from the API, not a
-- SQL-level operational kill-switch for server-side jobs. Neither
-- overlaps with the genuine gap this migration fills: an ops-controlled
-- switch SQL Server-side jobs/procedures can check directly, with no .NET
-- app tier in the loop at all (useful because these jobs run via SQL Agent /
-- Service Broker activation, independent of any API process being up).
--
-- Two flags are seeded, each with a real, already-built consumer (no
-- speculative flags):
--   1. [PlantSafety.ArchiveDB].System.FeatureFlag.'RetentionPurge.Enabled'
--      Checked by all 5 Phase 7 retention purge procedures (script 74)
--      before they run their DELETE. When disabled, a purge run logs an
--      Archival.ArchivalJob row with Status = 'Skipped' (distinct from
--      'Blocked', which is reserved for the ErrorLog no-delete-trigger
--      case) and returns without touching data.
--   2. [PlantSafety.IntegrationDB].System.FeatureFlag.
--      'ServiceBroker.ReplayProcessingEnabled'
--      Checked by the Phase 10 (script 100) target-side activation
--      procedure Integration.sp_ReplayRequestQueue_Activation at the top of
--      each receive iteration. When disabled, the procedure exits
--      immediately without issuing RECEIVE, so queued ReplayRequest
--      messages accumulate untouched (a true pause, not a data-dropping
--      no-op) until the flag is re-enabled.
--
-- Both existing procedures are altered via CREATE OR ALTER, additively: the
-- flag check is a new guard inserted at the top of each procedure body;
-- all pre-existing logic (retention lookup, DELETE, ArchivalJob logging,
-- Service Broker RECEIVE loop) is preserved unchanged.
--
-- Idempotency: table creation guarded by existence check; flag seeding uses
-- MERGE keyed on FlagName (does not overwrite a value an operator has since
-- changed); procedures use CREATE OR ALTER.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. System.FeatureFlag table + seed row, PlantSafety.ArchiveDB.
------------------------------------------------------------
USE [PlantSafety.ArchiveDB];
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'System' AND t.name = N'FeatureFlag')
BEGIN
    CREATE TABLE [System].[FeatureFlag] (
        FeatureFlagId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_System_FeatureFlag PRIMARY KEY CLUSTERED,
        FlagName       nvarchar(200)  NOT NULL,
        IsEnabled      bit            NOT NULL CONSTRAINT DF_System_FeatureFlag_IsEnabled DEFAULT (1),
        Description    nvarchar(1000) NULL,
        UpdatedUtc     datetime2(3)   NOT NULL CONSTRAINT DF_System_FeatureFlag_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy      nvarchar(128)  NOT NULL CONSTRAINT DF_System_FeatureFlag_UpdatedBy DEFAULT (SYSTEM_USER),
        CONSTRAINT UQ_System_FeatureFlag_FlagName UNIQUE (FlagName)
    );
END
GO

MERGE [System].[FeatureFlag] AS tgt
USING (SELECT N'RetentionPurge.Enabled' AS FlagName,
              CAST(1 AS bit) AS IsEnabled,
              N'Ops kill-switch for the Phase 7 retention purge stored procedures (sp_AuditLog_Purge / sp_ErrorLog_Purge / sp_IntegrationLogEntry_Purge / sp_ReplayRecord_Purge / sp_DiagnosticEvent_Purge). When set to 0, every purge proc logs a Skipped ArchivalJob row and returns without deleting any rows. Default ON (matches pre-existing behavior; this flag is additive, it does not change current default operation).' AS Description) AS src
ON tgt.FlagName = src.FlagName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (FlagName, IsEnabled, Description) VALUES (src.FlagName, src.IsEnabled, src.Description);
GO

------------------------------------------------------------
-- 2. System.FeatureFlag table + seed row, PlantSafety.IntegrationDB.
------------------------------------------------------------
USE [PlantSafety.IntegrationDB];
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'System' AND t.name = N'FeatureFlag')
BEGIN
    CREATE TABLE [System].[FeatureFlag] (
        FeatureFlagId  BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_System_FeatureFlag PRIMARY KEY CLUSTERED,
        FlagName       nvarchar(200)  NOT NULL,
        IsEnabled      bit            NOT NULL CONSTRAINT DF_System_FeatureFlag_IsEnabled DEFAULT (1),
        Description    nvarchar(1000) NULL,
        UpdatedUtc     datetime2(3)   NOT NULL CONSTRAINT DF_System_FeatureFlag_UpdatedUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedBy      nvarchar(128)  NOT NULL CONSTRAINT DF_System_FeatureFlag_UpdatedBy DEFAULT (SYSTEM_USER),
        CONSTRAINT UQ_System_FeatureFlag_FlagName UNIQUE (FlagName)
    );
END
GO

MERGE [System].[FeatureFlag] AS tgt
USING (SELECT N'ServiceBroker.ReplayProcessingEnabled' AS FlagName,
              CAST(1 AS bit) AS IsEnabled,
              N'Ops kill-switch for the Phase 10 Service Broker replay-request activation procedure (Integration.sp_ReplayRequestQueue_Activation). When set to 0, the activation procedure exits without RECEIVE-ing, so queued ReplayRequest messages accumulate untouched until re-enabled (pause, not drop). Default ON (matches pre-existing behavior).' AS Description) AS src
ON tgt.FlagName = src.FlagName
WHEN NOT MATCHED BY TARGET THEN
    INSERT (FlagName, IsEnabled, Description) VALUES (src.FlagName, src.IsEnabled, src.Description);
GO

------------------------------------------------------------
-- 3. Wire the flag into the 5 Phase 7 purge procedures (CREATE OR ALTER,
--    additive guard only -- all pre-existing logic unchanged).
------------------------------------------------------------

-- 3a. AuditDB.dbo.sp_AuditLog_Purge -----------------------------------------
USE [PlantSafety.AuditDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_AuditLog_Purge
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @JobId uniqueidentifier = NEWID();
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @RetentionDays int;
    DECLARE @CutoffUtc datetime2;
    DECLARE @RowsPurged int = 0;
    DECLARE @Started datetime2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM [PlantSafety.ArchiveDB].System.FeatureFlag WHERE FlagName = N'RetentionPurge.Enabled' AND IsEnabled = 1)
    BEGIN
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'AuditDB.dbo.AuditLog', N'Hot', N'Purged', N'Skipped', @Started, SYSUTCDATETIME(), N'Skipped: System.FeatureFlag ''RetentionPurge.Enabled'' is disabled in PlantSafety.ArchiveDB.', @JobId, SYSUTCDATETIME(), @System, 0);
        RETURN;
    END

    SELECT @RetentionDays = TransitionAfterDays
    FROM [PlantSafety.ArchiveDB].Archival.TierPolicy
    WHERE Zone = N'AuditDB.dbo.AuditLog' AND ISNULL(IsDeleted, 0) = 0;
    IF @RetentionDays IS NULL SET @RetentionDays = 90; -- placeholder default, see migration header

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    BEGIN TRY
        DELETE FROM dbo.AuditLog WHERE ChangeTime < @CutoffUtc;
        SET @RowsPurged = @@ROWCOUNT;

        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'AuditDB.dbo.AuditLog', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: ', @RowsPurged, N' row(s) purged older than ', @CutoffUtc), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'AuditDB.dbo.AuditLog', N'Hot', N'Purged', N'Failed', @Started, SYSUTCDATETIME(), LEFT(ERROR_MESSAGE(), 2000), @JobId, SYSUTCDATETIME(), @System, 0);
        THROW;
    END CATCH
END
GO

-- 3b. ErrorDB.dbo.sp_ErrorLog_Purge -----------------------------------------
USE [PlantSafety.ErrorDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.sp_ErrorLog_Purge
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @JobId uniqueidentifier = NEWID();
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @RetentionDays int;
    DECLARE @CutoffUtc datetime2;
    DECLARE @Started datetime2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM [PlantSafety.ArchiveDB].System.FeatureFlag WHERE FlagName = N'RetentionPurge.Enabled' AND IsEnabled = 1)
    BEGIN
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'ErrorDB.dbo.ErrorLog', N'Hot', N'Purged', N'Skipped', @Started, SYSUTCDATETIME(), N'Skipped: System.FeatureFlag ''RetentionPurge.Enabled'' is disabled in PlantSafety.ArchiveDB.', @JobId, SYSUTCDATETIME(), @System, 0);
        RETURN;
    END

    SELECT @RetentionDays = TransitionAfterDays
    FROM [PlantSafety.ArchiveDB].Archival.TierPolicy
    WHERE Zone = N'ErrorDB.dbo.ErrorLog' AND ISNULL(IsDeleted, 0) = 0;
    IF @RetentionDays IS NULL SET @RetentionDays = 90;

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    BEGIN TRY
        -- This DELETE is intentionally attempted (not skipped) so the
        -- existing TR_ErrorLog_NoDelete trigger's protection is genuinely
        -- exercised/verified on every run, not just assumed.
        DELETE FROM dbo.ErrorLog WHERE ErrorDate < @CutoffUtc;

        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'ErrorDB.dbo.ErrorLog', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: rows older than ', @CutoffUtc, N' purged.'), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'ErrorDB.dbo.ErrorLog', N'Hot', N'Purged', N'Blocked', @Started, SYSUTCDATETIME(),
             LEFT(CONCAT(N'BLOCKED by TR_ErrorLog_NoDelete (no-delete control on ErrorLog) -- retention policy vs. immutability control conflict requires a human decision before this can run. Underlying error: ', ERROR_MESSAGE()), 2000),
             @JobId, SYSUTCDATETIME(), @System, 0);
        -- Deliberately NOT re-throwing: a blocked purge is an expected,
        -- logged outcome here, not an unhandled failure.
    END CATCH
END
GO

-- 3c. IntegrationDB.Integration.sp_IntegrationLogEntry_Purge ----------------
USE [PlantSafety.IntegrationDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE Integration.sp_IntegrationLogEntry_Purge
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @JobId uniqueidentifier = NEWID();
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @RetentionDays int;
    DECLARE @CutoffUtc datetime2;
    DECLARE @RowsPurged int = 0;
    DECLARE @Started datetime2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM [PlantSafety.ArchiveDB].System.FeatureFlag WHERE FlagName = N'RetentionPurge.Enabled' AND IsEnabled = 1)
    BEGIN
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.IntegrationLogEntry', N'Hot', N'Purged', N'Skipped', @Started, SYSUTCDATETIME(), N'Skipped: System.FeatureFlag ''RetentionPurge.Enabled'' is disabled in PlantSafety.ArchiveDB.', @JobId, SYSUTCDATETIME(), @System, 0);
        RETURN;
    END

    SELECT @RetentionDays = TransitionAfterDays
    FROM [PlantSafety.ArchiveDB].Archival.TierPolicy
    WHERE Zone = N'IntegrationDB.Integration.IntegrationLogEntry' AND ISNULL(IsDeleted, 0) = 0;
    IF @RetentionDays IS NULL SET @RetentionDays = 90;

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    BEGIN TRY
        DELETE FROM Integration.IntegrationLogEntry WHERE TimestampUtc < @CutoffUtc;
        SET @RowsPurged = @@ROWCOUNT;

        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.IntegrationLogEntry', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: ', @RowsPurged, N' row(s) purged older than ', @CutoffUtc), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.IntegrationLogEntry', N'Hot', N'Purged', N'Failed', @Started, SYSUTCDATETIME(), LEFT(ERROR_MESSAGE(), 2000), @JobId, SYSUTCDATETIME(), @System, 0);
        THROW;
    END CATCH
END
GO

-- 3d. IntegrationDB.Integration.sp_ReplayRecord_Purge -----------------------
CREATE OR ALTER PROCEDURE Integration.sp_ReplayRecord_Purge
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @JobId uniqueidentifier = NEWID();
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @RetentionDays int;
    DECLARE @CutoffUtc datetime2;
    DECLARE @RowsPurged int = 0;
    DECLARE @Started datetime2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM [PlantSafety.ArchiveDB].System.FeatureFlag WHERE FlagName = N'RetentionPurge.Enabled' AND IsEnabled = 1)
    BEGIN
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.ReplayRecord', N'Hot', N'Purged', N'Skipped', @Started, SYSUTCDATETIME(), N'Skipped: System.FeatureFlag ''RetentionPurge.Enabled'' is disabled in PlantSafety.ArchiveDB.', @JobId, SYSUTCDATETIME(), @System, 0);
        RETURN;
    END

    SELECT @RetentionDays = TransitionAfterDays
    FROM [PlantSafety.ArchiveDB].Archival.TierPolicy
    WHERE Zone = N'IntegrationDB.Integration.ReplayRecord' AND ISNULL(IsDeleted, 0) = 0;
    IF @RetentionDays IS NULL SET @RetentionDays = 90;

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    BEGIN TRY
        DELETE FROM Integration.ReplayRecord WHERE TimestampUtc < @CutoffUtc;
        SET @RowsPurged = @@ROWCOUNT;

        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.ReplayRecord', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: ', @RowsPurged, N' row(s) purged older than ', @CutoffUtc), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'IntegrationDB.Integration.ReplayRecord', N'Hot', N'Purged', N'Failed', @Started, SYSUTCDATETIME(), LEFT(ERROR_MESSAGE(), 2000), @JobId, SYSUTCDATETIME(), @System, 0);
        THROW;
    END CATCH
END
GO

-- 3e. DiagnosticsDB.Diagnostics.sp_DiagnosticEvent_Purge --------------------
USE [PlantSafety.DiagnosticsDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE Diagnostics.sp_DiagnosticEvent_Purge
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @JobId uniqueidentifier = NEWID();
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @RetentionDays int;
    DECLARE @CutoffUtc datetime2;
    DECLARE @RowsPurged int = 0;
    DECLARE @Started datetime2 = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM [PlantSafety.ArchiveDB].System.FeatureFlag WHERE FlagName = N'RetentionPurge.Enabled' AND IsEnabled = 1)
    BEGIN
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'DiagnosticsDB.Diagnostics.DiagnosticEvent', N'Hot', N'Purged', N'Skipped', @Started, SYSUTCDATETIME(), N'Skipped: System.FeatureFlag ''RetentionPurge.Enabled'' is disabled in PlantSafety.ArchiveDB.', @JobId, SYSUTCDATETIME(), @System, 0);
        RETURN;
    END

    SELECT @RetentionDays = TransitionAfterDays
    FROM [PlantSafety.ArchiveDB].Archival.TierPolicy
    WHERE Zone = N'DiagnosticsDB.Diagnostics.DiagnosticEvent' AND ISNULL(IsDeleted, 0) = 0;
    IF @RetentionDays IS NULL SET @RetentionDays = 90;

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, SYSUTCDATETIME());

    BEGIN TRY
        DELETE FROM Diagnostics.DiagnosticEvent WHERE OccurredAtUtc < @CutoffUtc;
        SET @RowsPurged = @@ROWCOUNT;

        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'DiagnosticsDB.Diagnostics.DiagnosticEvent', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: ', @RowsPurged, N' row(s) purged older than ', @CutoffUtc), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'DiagnosticsDB.Diagnostics.DiagnosticEvent', N'Hot', N'Purged', N'Failed', @Started, SYSUTCDATETIME(), LEFT(ERROR_MESSAGE(), 2000), @JobId, SYSUTCDATETIME(), @System, 0);
        THROW;
    END CATCH
END
GO

------------------------------------------------------------
-- 4. Wire the flag into the Phase 10 Service Broker replay activation
--    procedure (CREATE OR ALTER, additive guard only).
------------------------------------------------------------
USE [PlantSafety.IntegrationDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE Integration.sp_ReplayRequestQueue_Activation
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ConversationHandle uniqueidentifier;
    DECLARE @MessageTypeName sysname;
    DECLARE @MessageBody nvarchar(MAX);
    DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000';
    DECLARE @Done bit = 0;
    -- NOTE: BREAK/CONTINUE are not permitted inside a TRY block (T-SQL
    -- restriction), so loop exit is controlled via @Done rather than BREAK.

    WHILE @Done = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [PlantSafety.IntegrationDB].System.FeatureFlag WHERE FlagName = N'ServiceBroker.ReplayProcessingEnabled' AND IsEnabled = 1)
        BEGIN
            -- Paused: exit without RECEIVE-ing so queued messages remain
            -- untouched (activation will simply be invoked again later).
            SET @Done = 1;
            BREAK;
        END

        BEGIN TRY
            BEGIN TRANSACTION;

            WAITFOR (
                RECEIVE TOP(1)
                    @ConversationHandle = conversation_handle,
                    @MessageTypeName    = message_type_name,
                    @MessageBody        = CAST(message_body AS nvarchar(MAX))
                FROM Integration.ReplayRequestQueue
            ), TIMEOUT 5000;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @Done = 1;
                COMMIT TRANSACTION;
            END
            ELSE
            BEGIN

            IF @MessageTypeName = N'//PlantSafety/Integration/ReplayRequest'
            BEGIN
                DECLARE @ReplayRecordId uniqueidentifier = TRY_CAST(@MessageBody AS uniqueidentifier);
                DECLARE @Exists bit = 0;
                DECLARE @PayloadOk bit = 0;

                IF @ReplayRecordId IS NOT NULL
                BEGIN
                    SELECT @Exists = 1, @PayloadOk = CASE WHEN LEN(SerializedPayload) > 0 THEN 1 ELSE 0 END
                    FROM Integration.ReplayRecord
                    WHERE ReplayRecordId = @ReplayRecordId AND ISNULL(IsDeleted, 0) = 0;
                END

                IF @Exists = 1 AND @PayloadOk = 1
                BEGIN
                    UPDATE Integration.ReplayRecord
                        SET Status = N'Completed', ProcessedUtc = SYSUTCDATETIME(), FailureReason = NULL
                        WHERE ReplayRecordId = @ReplayRecordId;

                    INSERT INTO Integration.IntegrationLogEntry
                        (IntegrationLogEntryId, CorrelationId, CausationId, MessageId, TraceId, SpanId,
                         TimestampUtc, ElapsedMs, OperationName, ModuleName, Environment, Version, Success,
                         CreatedUtc, CreatedBy, IsDeleted)
                    SELECT NEWID(), CorrelationId, CausationId, MessageId, CONVERT(nvarchar(36), @ReplayRecordId), N'0',
                           SYSUTCDATETIME(), 0, N'ReplayRecord.Processed', N'Integration', N'Production', N'1.0', 1,
                           SYSUTCDATETIME(), @System, 0
                    FROM Integration.ReplayRecord WHERE ReplayRecordId = @ReplayRecordId;
                END
                ELSE
                BEGIN
                    IF @ReplayRecordId IS NOT NULL
                        UPDATE Integration.ReplayRecord
                            SET Status = N'Failed', ProcessedUtc = SYSUTCDATETIME(),
                                FailureReason = N'ReplayRecord not found, soft-deleted, or payload empty at processing time.'
                            WHERE ReplayRecordId = @ReplayRecordId;
                END;
                -- NOTE: the semicolon above is required -- SEND ON
                -- CONVERSATION fails to parse (Msg 102, "Incorrect syntax
                -- near 'SEND'") if immediately preceded by an
                -- unterminated IF/ELSE END block; live-verified 2026-07-19.

                SEND ON CONVERSATION @ConversationHandle
                    MESSAGE TYPE [//PlantSafety/Integration/ReplayResponse]
                    (CAST(ISNULL(@ReplayRecordId, '00000000-0000-0000-0000-000000000000') AS nvarchar(36)));

                END CONVERSATION @ConversationHandle;
            END
            ELSE IF @MessageTypeName = N'https://schemas.microsoft.com/SQL/ServiceBroker/EndDialog'
            BEGIN
                END CONVERSATION @ConversationHandle;
            END
            ELSE IF @MessageTypeName = N'https://schemas.microsoft.com/SQL/ServiceBroker/Error'
            BEGIN
                END CONVERSATION @ConversationHandle;
            END

            COMMIT TRANSACTION;
            END
        END TRY
        BEGIN CATCH
            IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

            DECLARE @ErrorContext nvarchar(400) = CONCAT(N'Queue=Integration.ReplayRequestQueue; ConversationHandle=', @ConversationHandle);
            DECLARE @ErrorDetails nvarchar(max) = CONCAT(
                N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
                N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog
                (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                 ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES
                (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                 SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
            SET @Done = 1;
        END CATCH
    END
END
GO

------------------------------------------------------------
-- 5. Document the new table/columns.
------------------------------------------------------------
USE [PlantSafety.ArchiveDB];
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'System.FeatureFlag') AND ep.minor_id = 0 AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Ops-controlled DB-level feature/kill switches, checked directly by SQL Server-side jobs/procedures (independent of the .NET app tier). See 144-feature-flags.sql for the genuine-gap rationale and the full list of consumers.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'FeatureFlag';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ops-controlled DB-level feature/kill switches, checked directly by SQL Server-side jobs/procedures (independent of the .NET app tier). See 144-feature-flags.sql for the genuine-gap rationale and the full list of consumers.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'FeatureFlag';
GO
USE [PlantSafety.IntegrationDB];
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'System.FeatureFlag') AND ep.minor_id = 0 AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Ops-controlled DB-level feature/kill switches, checked directly by SQL Server-side jobs/procedures (independent of the .NET app tier). See 144-feature-flags.sql for the genuine-gap rationale and the full list of consumers.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'FeatureFlag';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ops-controlled DB-level feature/kill switches, checked directly by SQL Server-side jobs/procedures (independent of the .NET app tier). See 144-feature-flags.sql for the genuine-gap rationale and the full list of consumers.', @level0type=N'SCHEMA', @level0name=N'System', @level1type=N'TABLE', @level1name=N'FeatureFlag';
GO

------------------------------------------------------------
-- 6. Verification
------------------------------------------------------------
SELECT DB_NAME() AS DbName, FlagName, IsEnabled, Description FROM [PlantSafety.ArchiveDB].System.FeatureFlag;
SELECT DB_NAME() AS DbName, FlagName, IsEnabled, Description FROM [PlantSafety.IntegrationDB].System.FeatureFlag;
GO

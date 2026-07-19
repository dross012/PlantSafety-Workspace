-- =============================================
-- Migration:     74-retention-archival.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Retention/archival enforcement (Governance remediation
--                item #3). Wires PlantSafety.ArchiveDB's Archival.
--                ArchivalJob / Archival.TierPolicy tables to the 5 real
--                high-volume append-only log tables, and registers SQL
--                Server Agent job definitions to run them on a schedule.
--
-- *** RETENTION WINDOW IS A PLACEHOLDER ***
-- 90 days is used as a generic, commonly-seen default for operational log
-- retention. This is NOT sourced from any confirmed regulatory or
-- contractual requirement for PlantSafety -- it is a sensible starting
-- point the user must confirm against real compliance/legal requirements
-- (e.g. incident-adjacent logs may need to align with OSHA/HSE record
-- retention rules, which are frequently 5+ years, not 90 days -- these
-- 5 tables are pure technical/operational logs, not the incident records
-- themselves, but confirm before relying on 90 days in production).
--
-- *** IMPORTANT CONFLICT FLAGGED: ErrorDB.dbo.ErrorLog ***
-- ErrorDB.dbo.ErrorLog has an existing trigger `TR_ErrorLog_NoDelete`
-- (INSTEAD OF DELETE, RAISERROR) that unconditionally blocks all deletes,
-- including ones issued by a retention purge. Per the stricter-compliance
-- rule (never loosen a control to make a check pass), this migration does
-- NOT disable/bypass that trigger. sp_ErrorLog_Purge below will always be
-- blocked by design until a human makes an explicit decision on how to
-- reconcile "ErrorLog rows are immutable/no-delete" with "ErrorLog rows
-- must be purged after N days" (e.g. add an explicit archive-then-copy
-- step before a deliberately trigger-aware delete, or scope the no-delete
-- trigger to protect only recent rows). Every run of sp_ErrorLog_Purge
-- logs a 'Blocked' ArchivalJob row documenting this rather than silently
-- succeeding or silently disabling the control.
--
-- *** SQL Server Agent status (live-verified 2026-07-19): STOPPED,
-- startup type DISABLED *** -- the job definitions below are created
-- regardless (metadata-only operation against msdb, does not require the
-- Agent service to be running), but they will NOT actually execute on
-- schedule until a human starts the "SQL Server Agent (SQLEXPRESS)"
-- Windows service and sets its startup type to at least Manual (Automatic
-- recommended for a real deployment).
--
-- Idempotency: all TierPolicy seeds, stored procedures (CREATE OR ALTER),
-- and Agent job registrations are existence-checked / safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. Seed Archival.TierPolicy rows (one per log table), 90-day placeholder.
------------------------------------------------------------
USE [PlantSafety.ArchiveDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @Now datetime2 = SYSUTCDATETIME();
DECLARE @System uniqueidentifier = '00000000-0000-0000-0000-000000000000'; -- automation/system actor, matches existing platform convention for system-originated rows

MERGE Archival.TierPolicy AS tgt
USING (VALUES
    (N'AuditDB.dbo.AuditLog',                       N'Hot', N'Purged', 90),
    (N'ErrorDB.dbo.ErrorLog',                        N'Hot', N'Purged', 90),
    (N'IntegrationDB.Integration.IntegrationLogEntry',N'Hot', N'Purged', 90),
    (N'IntegrationDB.Integration.ReplayRecord',       N'Hot', N'Purged', 90),
    (N'DiagnosticsDB.Diagnostics.DiagnosticEvent',    N'Hot', N'Purged', 90)
) AS src (Zone, SourceTier, TargetTier, TransitionAfterDays)
ON tgt.Zone = src.Zone AND ISNULL(tgt.IsDeleted, 0) = 0
WHEN NOT MATCHED THEN
    INSERT (TierPolicyId, Zone, SourceTier, TargetTier, TransitionAfterDays, CreatedUtc, CreatedBy, IsDeleted)
    VALUES (NEWID(), src.Zone, src.SourceTier, src.TargetTier, src.TransitionAfterDays, @Now, @System, 0);
GO

------------------------------------------------------------
-- 2. Purge stored procedures (one per high-volume log table).
--    Each: reads TransitionAfterDays from Archival.TierPolicy by Zone
--    (defaulting to 90 if the policy row is somehow missing), deletes rows
--    older than the cutoff, and logs an Archival.ArchivalJob row in
--    PlantSafety.ArchiveDB documenting the run.
------------------------------------------------------------

-- 2a. AuditDB.dbo.AuditLog -----------------------------------------------
USE [PlantSafety.AuditDB];
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

-- 2b. ErrorDB.dbo.ErrorLog -- BLOCKED by TR_ErrorLog_NoDelete, by design --
USE [PlantSafety.ErrorDB];
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

        -- If we ever get here, the no-delete trigger has been removed/
        -- changed since this proc was written -- log success plainly.
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'ErrorDB.dbo.ErrorLog', N'Hot', N'Purged', N'Completed', @Started, SYSUTCDATETIME(), CONCAT(N'SUCCESS: rows older than ', @CutoffUtc, N' purged.'), @JobId, SYSUTCDATETIME(), @System, 0);
    END TRY
    BEGIN CATCH
        -- Expected path: TR_ErrorLog_NoDelete blocks the DELETE. Log this
        -- as a 'Blocked' job, not a silent no-op and not a forced bypass.
        INSERT INTO [PlantSafety.ArchiveDB].Archival.ArchivalJob
            (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CreatedUtc, CreatedBy, IsDeleted)
        VALUES
            (@JobId, @JobId, @System, N'ErrorDB.dbo.ErrorLog', N'Hot', N'Purged', N'Blocked', @Started, SYSUTCDATETIME(),
             LEFT(CONCAT(N'BLOCKED by TR_ErrorLog_NoDelete (no-delete control on ErrorLog) -- retention policy vs. immutability control conflict requires a human decision before this can run. Underlying error: ', ERROR_MESSAGE()), 2000),
             @JobId, SYSUTCDATETIME(), @System, 0);
        -- Deliberately NOT re-throwing: a blocked purge is an expected,
        -- logged outcome here, not an unhandled failure that should crash
        -- the caller/Agent job.
    END CATCH
END
GO

-- 2c. IntegrationDB.Integration.IntegrationLogEntry ----------------------
USE [PlantSafety.IntegrationDB];
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

-- 2d. IntegrationDB.Integration.ReplayRecord ------------------------------
USE [PlantSafety.IntegrationDB];
GO
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

-- 2e. DiagnosticsDB.Diagnostics.DiagnosticEvent ---------------------------
USE [PlantSafety.DiagnosticsDB];
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
-- 3. SQL Server Agent job registrations.
--    NOTE: msdb job metadata can be created even while the Agent service is
--    stopped; the jobs simply will not fire until the service is running.
--    Schedule: daily at 02:00 local server time -- a placeholder cadence,
--    adjust to actual operational windows as needed.
------------------------------------------------------------
USE [msdb];
GO

DECLARE @JobDefs TABLE (JobName sysname, DbName sysname, ProcName sysname);
INSERT INTO @JobDefs VALUES
    (N'PlantSafety - Purge AuditLog',            N'PlantSafety.AuditDB',       N'dbo.sp_AuditLog_Purge'),
    (N'PlantSafety - Purge ErrorLog',             N'PlantSafety.ErrorDB',       N'dbo.sp_ErrorLog_Purge'),
    (N'PlantSafety - Purge IntegrationLogEntry',  N'PlantSafety.IntegrationDB', N'Integration.sp_IntegrationLogEntry_Purge'),
    (N'PlantSafety - Purge ReplayRecord',         N'PlantSafety.IntegrationDB', N'Integration.sp_ReplayRecord_Purge'),
    (N'PlantSafety - Purge DiagnosticEvent',      N'PlantSafety.DiagnosticsDB', N'Diagnostics.sp_DiagnosticEvent_Purge');

DECLARE @JobName sysname, @DbName sysname, @ProcName sysname, @JobId uniqueidentifier;
DECLARE @ScheduleName sysname = N'PlantSafety - Daily Retention Purge 02:00';

IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysschedules WHERE name = @ScheduleName)
BEGIN
    EXEC msdb.dbo.sp_add_schedule
        @schedule_name = @ScheduleName,
        @freq_type = 4,          -- daily
        @freq_interval = 1,
        @active_start_time = 020000; -- 02:00:00
END

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT JobName, DbName, ProcName FROM @JobDefs;
OPEN cur;
FETCH NEXT FROM cur INTO @JobName, @DbName, @ProcName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @JobId = NULL;
    IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @JobName)
    BEGIN
        EXEC msdb.dbo.sp_add_job
            @job_name = @JobName,
            @enabled = 1,
            @description = N'PlantSafety Phase 7 governance: retention purge (90-day placeholder, see 74-retention-archival.sql). Registered by Governance Enforcer agent 2026-07-19.',
            @owner_login_name = N'sa',
            @job_id = @JobId OUTPUT;

        EXEC msdb.dbo.sp_add_jobstep
            @job_id = @JobId,
            @step_name = N'Run purge procedure',
            @subsystem = N'TSQL',
            @database_name = @DbName,
            @command = @ProcName,
            @on_success_action = 1, -- quit with success
            @on_fail_action = 2;    -- quit with failure

        EXEC msdb.dbo.sp_attach_schedule
            @job_id = @JobId,
            @schedule_name = @ScheduleName;

        EXEC msdb.dbo.sp_add_jobserver
            @job_id = @JobId,
            @server_name = N'(local)';

        PRINT 'Registered SQL Agent job: ' + @JobName;
    END
    ELSE
        PRINT 'SQL Agent job already exists: ' + @JobName + ' -- skipped.';

    FETCH NEXT FROM cur INTO @JobName, @DbName, @ProcName;
END
CLOSE cur;
DEALLOCATE cur;
GO

------------------------------------------------------------
-- 4. Verification
------------------------------------------------------------
SELECT name, enabled FROM msdb.dbo.sysjobs WHERE name LIKE N'PlantSafety - Purge%' ORDER BY name;
GO
SELECT Zone, SourceTier, TargetTier, TransitionAfterDays FROM [PlantSafety.ArchiveDB].Archival.TierPolicy ORDER BY Zone;
GO


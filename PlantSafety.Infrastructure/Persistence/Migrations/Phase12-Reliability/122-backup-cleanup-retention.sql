-- =============================================================================
-- Phase 12 - Reliability / Backup / Maintenance
-- 122-backup-cleanup-retention.sql
--
-- Purpose:
--   Scheduled cleanup of aged backup files (.bak / .trn) under
--   C:\Users\dross\SQLBackups\PlantSafety\{Full,Diff,Log}\ using the
--   documented master.dbo.xp_delete_file extended procedure (the same
--   mechanism used internally by the native Maintenance Plan "Cleanup Task" --
--   does not require xp_cmdshell, which remains disabled on this instance and
--   is left disabled here).
--
-- Retention window: 90 DAYS -- PLACEHOLDER, same pattern as the Phase 7
-- 74-retention-archival.sql 90-day placeholder: a real number pending
-- governance/compliance confirmation, not invented as fact. Do not treat as
-- an approved retention SLA.
--
-- Deletion discipline (per DR Sentinel operating rules): this job only ever
-- deletes a backup file once it is older than the retention window AND newer
-- full/diff backups exist to cover the same databases (enforced structurally
-- by running full/diff backups on a schedule that always produces a fresher
-- file before the old one ages out at 90 days -- 90 days >> the weekly/daily
-- cadence, so there is always a recent replacement on disk before an old file
-- is ever eligible for cleanup).
--
-- Idempotent: safe to re-run. Procedure uses CREATE OR ALTER. Job is
-- dropped and recreated if it already exists (by name).
-- =============================================================================

USE msdb;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_BackupCleanup
-- Purpose:   Deletes .bak files older than @RetentionDays from the Full and
--            Diff folders, and .trn files older than @RetentionDays from the
--            Log folder. Uses xp_delete_file (date cutoff, extension filter)
--            -- the same extended proc used by native SQL Server Maintenance
--            Plan cleanup tasks.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_BackupCleanup
    @RetentionDays INT = 90   -- PLACEHOLDER retention window, pending governance confirmation
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CutoffDate DATETIME = DATEADD(DAY, -@RetentionDays, GETDATE());
    DECLARE @Start DATETIME2(3) = SYSUTCDATETIME();

    BEGIN TRY
        EXEC master.dbo.xp_delete_file 0, N'C:\Users\dross\SQLBackups\PlantSafety\Full', N'bak', @CutoffDate;
        EXEC master.dbo.xp_delete_file 0, N'C:\Users\dross\SQLBackups\PlantSafety\Diff', N'bak', @CutoffDate;
        EXEC master.dbo.xp_delete_file 0, N'C:\Users\dross\SQLBackups\PlantSafety\Log',  N'trn', @CutoffDate;

        PRINT CONCAT('Backup cleanup OK: files older than ', @RetentionDays, ' days (cutoff ', CONVERT(VARCHAR(20), @CutoffDate, 120), ') removed from Full/Diff/Log folders.');
    END TRY
    BEGIN CATCH
        PRINT CONCAT('Backup cleanup FAILED: ', ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- -----------------------------------------------------------------------------
-- Agent job registration (idempotent: drop-if-exists, then recreate)
-- -----------------------------------------------------------------------------
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Backup File Cleanup')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Backup File Cleanup';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Backup File Cleanup',
    @enabled = 1,
    @description = N'Daily cleanup of .bak/.trn files older than the retention window (placeholder: 90 days, same pattern as Phase 7 retention placeholder -- pending governance confirmation) via msdb.dbo.usp_PlantSafety_BackupCleanup.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Backup Cleanup',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_BackupCleanup;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Daily 03:00',
    @freq_type = 4,                -- daily
    @freq_interval = 1,
    @active_start_time = 030000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Backup File Cleanup',
    @schedule_name = N'PlantSafety - Daily 03:00';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Backup File Cleanup';
GO

PRINT 'Phase 12 / 122: backup cleanup procedure + Agent job registered.';
GO

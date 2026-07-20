-- =============================================================================
-- Phase 12 - Reliability / Backup / Maintenance
-- 120-backup-jobs-full-diff-log.sql
--
-- Purpose:
--   Cross-database backup utility procedures + SQL Server Agent job registration
--   for FULL, DIFFERENTIAL, and (recovery-model-permitting) LOG backups across
--   every live PlantSafety.* database.
--
-- Scope note (No-Guessing Rule):
--   Live-verified via sys.databases on 2026-07-19 there are 18 PlantSafety.*
--   databases on this instance (AuditDB, ArchiveDB, CloudIdentityDB, CoreDB,
--   DiagnosticsDB, ErrorDB, ExportImportDB, IntegrationDB, MaintenanceDB,
--   NotificationsDB, PredictiveDB, ProductionDB, ReportingDB, RoutingDB,
--   SandboxDB, SchedulingDB, TestHarnessDB, ToolingDB) -- more than the 10
--   named in the original task brief. Per canonical-templates.md and the
--   No-Guessing Rule, these procedures target the database list DYNAMICALLY
--   (sys.databases WHERE name LIKE N'PlantSafety.%' AND state_desc = 'ONLINE'
--   AND database_id > 4) rather than a hardcoded list, so backup coverage
--   stays correct as databases are added/removed without a migration edit.
--
-- Recovery model note (checked live 2026-07-19):
--   ALL 18 PlantSafety.* databases are currently in SIMPLE recovery model
--   (confirmed via sys.databases.recovery_model_desc). SIMPLE recovery does
--   NOT support transaction log backups. usp_PlantSafety_BackupLog below is
--   registered and idempotent, but will SKIP (and log a reason, not an error)
--   any database still in SIMPLE model. Switching any database to FULL
--   recovery is an explicit governance/business decision (it changes log
--   growth behavior and requires an ongoing log-backup cadence to avoid an
--   unbounded log) and is NOT made here -- flagged as a placeholder decision
--   requiring human confirmation, consistent with the No-Guessing Rule.
--
-- Backup destination: C:\Users\dross\SQLBackups\PlantSafety\{Full,Diff,Log}\
--   (created out-of-band; NT Service\MSSQL$SQLEXPRESS granted Full Control via
--   icacls -- verified live by executing real backups below.)
--
-- EDITION-BLOCKED (Express): BACKUP ... WITH COMPRESSION raises Msg 1844 on
-- SQL Server Express ("BACKUP DATABASE WITH COMPRESSION is not supported on
-- Express Edition") -- native backup compression is a Standard/Enterprise
-- feature. Confirmed live 2026-07-19. These procedures therefore do NOT pass
-- COMPRESSION; backups are uncompressed .bak/.trn files. See
-- ha-dr-future-work.md for what upgrading to Standard/Enterprise would unlock
-- here (smaller backup files, faster I/O-bound backup/restore).
--
-- Idempotent: safe to re-run. Procedures use CREATE OR ALTER. Jobs are
-- dropped and recreated if they already exist (by name).
-- =============================================================================

USE msdb;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- -----------------------------------------------------------------------------
-- Shared backup-path convention (kept in sync across all three procs below)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    RAISERROR('dbo schema missing from msdb -- unexpected.', 16, 1);
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_BackupFull
-- Purpose:   FULL backup, checksummed (uncompressed -- COMPRESSION not supported on Express), for every live
--            PlantSafety.* database. Cross-database platform-utility object,
--            hence the usp_ prefix per canonical-templates.md naming rule.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_BackupFull
    @BackupRootPath NVARCHAR(260) = N'C:\Users\dross\SQLBackups\PlantSafety\Full\'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @FilePath NVARCHAR(400);
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Stamp NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(19), SYSUTCDATETIME(), 120), '-', ''), ':', ''), ' ', '_');

    DECLARE db_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT name FROM sys.databases
        WHERE name LIKE N'PlantSafety.%'
          AND state_desc = 'ONLINE'
          AND database_id > 4
        ORDER BY name;

    OPEN db_cursor;
    FETCH NEXT FROM db_cursor INTO @DbName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @FilePath = @BackupRootPath + @DbName + N'_FULL_' + @Stamp + N'.bak';
        SET @Sql = N'BACKUP DATABASE ' + QUOTENAME(@DbName) +
                   N' TO DISK = ''' + @FilePath + N'''' +
                   N' WITH CHECKSUM, INIT, NAME = ''' + @DbName + N'-Full-' + @Stamp + N''';';
        BEGIN TRY
            EXEC (@Sql);
            PRINT CONCAT('FULL backup OK: ', @DbName, ' -> ', @FilePath);
        END TRY
        BEGIN CATCH
            PRINT CONCAT('FULL backup FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
            THROW;
        END CATCH

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_BackupDiff
-- Purpose:   DIFFERENTIAL backup for every live PlantSafety.* database.
--            Skips (with a message, not an error) any database that has no
--            prior full backup, since a differential requires a full base.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_BackupDiff
    @BackupRootPath NVARCHAR(260) = N'C:\Users\dross\SQLBackups\PlantSafety\Diff\'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @FilePath NVARCHAR(400);
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Stamp NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(19), SYSUTCDATETIME(), 120), '-', ''), ':', ''), ' ', '_');
    DECLARE @HasFullBackup BIT;

    DECLARE db_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT name FROM sys.databases
        WHERE name LIKE N'PlantSafety.%'
          AND state_desc = 'ONLINE'
          AND database_id > 4
        ORDER BY name;

    OPEN db_cursor;
    FETCH NEXT FROM db_cursor INTO @DbName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @HasFullBackup = CASE WHEN EXISTS (
            SELECT 1 FROM msdb.dbo.backupset bs
            WHERE bs.database_name = @DbName AND bs.type = 'D'
        ) THEN 1 ELSE 0 END;

        IF @HasFullBackup = 0
        BEGIN
            PRINT CONCAT('DIFF backup SKIPPED (no prior FULL backup found): ', @DbName);
        END
        ELSE
        BEGIN
            SET @FilePath = @BackupRootPath + @DbName + N'_DIFF_' + @Stamp + N'.bak';
            SET @Sql = N'BACKUP DATABASE ' + QUOTENAME(@DbName) +
                       N' TO DISK = ''' + @FilePath + N'''' +
                       N' WITH DIFFERENTIAL, CHECKSUM, INIT, NAME = ''' + @DbName + N'-Diff-' + @Stamp + N''';';
            BEGIN TRY
                EXEC (@Sql);
                PRINT CONCAT('DIFF backup OK: ', @DbName, ' -> ', @FilePath);
            END TRY
            BEGIN CATCH
                PRINT CONCAT('DIFF backup FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
                THROW;
            END CATCH
        END

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_BackupLog
-- Purpose:   Transaction LOG backup for every live PlantSafety.* database
--            THAT IS IN FULL (or BULK_LOGGED) recovery model. As of
--            2026-07-19 all 18 PlantSafety.* databases are SIMPLE, so this
--            proc will currently skip every database and log why -- it is
--            registered now so the job exists and is proven wired up, ready
--            to activate the moment any database's recovery model is
--            deliberately changed to FULL by a human decision.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_BackupLog
    @BackupRootPath NVARCHAR(260) = N'C:\Users\dross\SQLBackups\PlantSafety\Log\'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @RecoveryModel NVARCHAR(60);
    DECLARE @FilePath NVARCHAR(400);
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Stamp NVARCHAR(20) = REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(19), SYSUTCDATETIME(), 120), '-', ''), ':', ''), ' ', '_');

    DECLARE db_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT name, recovery_model_desc FROM sys.databases
        WHERE name LIKE N'PlantSafety.%'
          AND state_desc = 'ONLINE'
          AND database_id > 4
        ORDER BY name;

    OPEN db_cursor;
    FETCH NEXT FROM db_cursor INTO @DbName, @RecoveryModel;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @RecoveryModel = 'SIMPLE'
        BEGIN
            PRINT CONCAT('LOG backup SKIPPED (recovery model = SIMPLE, log backups not supported): ', @DbName);
        END
        ELSE
        BEGIN
            SET @FilePath = @BackupRootPath + @DbName + N'_LOG_' + @Stamp + N'.trn';
            SET @Sql = N'BACKUP LOG ' + QUOTENAME(@DbName) +
                       N' TO DISK = ''' + @FilePath + N'''' +
                       N' WITH CHECKSUM, INIT, NAME = ''' + @DbName + N'-Log-' + @Stamp + N''';';
            BEGIN TRY
                EXEC (@Sql);
                PRINT CONCAT('LOG backup OK: ', @DbName, ' -> ', @FilePath);
            END TRY
            BEGIN CATCH
                PRINT CONCAT('LOG backup FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
                THROW;
            END CATCH
        END

        FETCH NEXT FROM db_cursor INTO @DbName, @RecoveryModel;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- -----------------------------------------------------------------------------
-- Agent job registration (idempotent: drop-if-exists, then recreate)
-- -----------------------------------------------------------------------------
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

-- 1) Full backup -- weekly, Sunday 01:00
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Full Backup (All Databases)')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Full Backup (All Databases)';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Full Backup (All Databases)',
    @enabled = 1,
    @description = N'Weekly FULL, checksummed (uncompressed -- Express edition) backup of every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_BackupFull.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Full Backup',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_BackupFull;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Weekly Sunday 01:00',
    @freq_type = 8,               -- weekly
    @freq_interval = 1,           -- Sunday
    @freq_recurrence_factor = 1,
    @active_start_time = 010000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Full Backup (All Databases)',
    @schedule_name = N'PlantSafety - Weekly Sunday 01:00';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Full Backup (All Databases)';
GO

-- 2) Differential backup -- daily (Mon-Sat) 01:00
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Differential Backup (All Databases)')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Differential Backup (All Databases)';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Differential Backup (All Databases)',
    @enabled = 1,
    @description = N'Daily (Mon-Sat) DIFFERENTIAL, checksummed (uncompressed -- Express edition) backup of every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_BackupDiff. Sunday is reserved for the FULL backup instead.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Differential Backup',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_BackupDiff;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Daily Mon-Sat 01:00',
    @freq_type = 8,               -- weekly
    @freq_interval = 126,         -- Mon(2)+Tue(4)+Wed(8)+Thu(16)+Fri(32)+Sat(64) = 126
    @freq_recurrence_factor = 1,
    @active_start_time = 010000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Differential Backup (All Databases)',
    @schedule_name = N'PlantSafety - Daily Mon-Sat 01:00';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Differential Backup (All Databases)';
GO

-- 3) Log backup -- every 15 minutes (registered now; will no-op on all-SIMPLE
--    databases until/unless a database is deliberately switched to FULL)
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Transaction Log Backup (All Databases)')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Transaction Log Backup (All Databases)';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Transaction Log Backup (All Databases)',
    @enabled = 1,
    @description = N'Every-15-minute LOG backup via msdb.dbo.usp_PlantSafety_BackupLog. NOTE: as of 2026-07-19 all PlantSafety.* databases are SIMPLE recovery model, so this job currently skips every database (prints a reason, does not error). It activates automatically for any database a human deliberately switches to FULL recovery -- placeholder cadence (15 min), not a confirmed RPO target.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Log Backup',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_BackupLog;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Every 15 Minutes',
    @freq_type = 4,                -- daily
    @freq_interval = 1,
    @freq_subday_type = 4,         -- minutes
    @freq_subday_interval = 15,
    @active_start_time = 000000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Transaction Log Backup (All Databases)',
    @schedule_name = N'PlantSafety - Every 15 Minutes';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Transaction Log Backup (All Databases)';
GO

PRINT 'Phase 12 / 120: backup procedures + Agent jobs registered.';
GO

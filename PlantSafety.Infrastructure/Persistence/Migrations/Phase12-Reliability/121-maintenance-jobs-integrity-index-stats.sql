-- =============================================================================
-- Phase 12 - Reliability / Backup / Maintenance
-- 121-maintenance-jobs-integrity-index-stats.sql
--
-- Purpose:
--   Cross-database maintenance utility procedures + SQL Server Agent job
--   registration for integrity checks (DBCC CHECKDB), index maintenance
--   (rebuild/reorganize by fragmentation threshold), and statistics updates,
--   across every live PlantSafety.* database. Same dynamic-inventory approach
--   as 120-backup-jobs-full-diff-log.sql (sys.databases WHERE name LIKE
--   N'PlantSafety.%' AND state_desc = 'ONLINE' AND database_id > 4).
--
-- Logging: results are written to msdb.dbo.PlantSafety_MaintenanceLog (created
-- below if missing) so runs are auditable without depending on SQL Agent job
-- history alone (useful since the Agent service is currently stopped).
--
-- Fragmentation thresholds (placeholder, Microsoft-documented conventional
-- values, not a confirmed PlantSafety-specific SLA -- flag for governance
-- confirmation same as the Phase 7 90-day retention placeholder):
--   avg_fragmentation_in_percent >= 30  -> REBUILD
--   avg_fragmentation_in_percent >= 5 and < 30 -> REORGANIZE
--   avg_fragmentation_in_percent < 5   -> no action
--   page_count < 1000 is skipped (fragmentation on tiny indexes is noise)
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantSafety_MaintenanceLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PlantSafety_MaintenanceLog
    (
        MaintenanceLogId   BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TaskType           VARCHAR(30)   NOT NULL,   -- 'CHECKDB' | 'INDEX_MAINT' | 'UPDATE_STATS'
        DatabaseName       SYSNAME       NOT NULL,
        ObjectDetail       NVARCHAR(400) NULL,        -- e.g. index/table name for index maint
        StartUtc           DATETIME2(3)  NOT NULL,
        EndUtc             DATETIME2(3)  NULL,
        Result             VARCHAR(20)   NOT NULL,    -- 'OK' | 'FAILED' | 'SKIPPED'
        Details            NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_PlantSafety_MaintenanceLog_TaskType_DatabaseName
        ON dbo.PlantSafety_MaintenanceLog (TaskType, DatabaseName, StartUtc DESC);
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_IntegrityCheck
-- Purpose:   DBCC CHECKDB (no data-modifying repair options) against every
--            live PlantSafety.* database. Read-only / safe.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_IntegrityCheck
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Start DATETIME2(3);
    DECLARE @LogId BIGINT;

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
        SET @Start = SYSUTCDATETIME();
        INSERT INTO dbo.PlantSafety_MaintenanceLog (TaskType, DatabaseName, StartUtc, Result, Details)
        VALUES ('CHECKDB', @DbName, @Start, 'OK', NULL);
        SET @LogId = SCOPE_IDENTITY();

        SET @Sql = N'DBCC CHECKDB(' + QUOTENAME(@DbName, '''') + N') WITH NO_INFOMSGS, ALL_ERRORMSGS;';

        BEGIN TRY
            EXEC (@Sql);
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'OK', Details = 'DBCC CHECKDB completed with no errors reported.'
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('CHECKDB OK: ', @DbName);
        END TRY
        BEGIN CATCH
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'FAILED', Details = ERROR_MESSAGE()
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('CHECKDB FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
        END CATCH

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_IndexMaintenance
-- Purpose:   Loops every live PlantSafety.* database, inspects
--            sys.dm_db_index_physical_stats, and REBUILDs (>=30% frag) or
--            REORGANIZEs (5-30% frag) indexes with page_count >= 1000.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_IndexMaintenance
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Start DATETIME2(3);
    DECLARE @LogId BIGINT;

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
        SET @Start = SYSUTCDATETIME();
        INSERT INTO dbo.PlantSafety_MaintenanceLog (TaskType, DatabaseName, StartUtc, Result, Details)
        VALUES ('INDEX_MAINT', @DbName, @Start, 'OK', NULL);
        SET @LogId = SCOPE_IDENTITY();

        -- Per-database dynamic SQL: build & run a cursor over fragmented indexes
        -- inside that database's own context (dm_db_index_physical_stats needs
        -- to be queried against the target DB).
        SET @Sql = N'
        USE ' + QUOTENAME(@DbName) + N';
        DECLARE @SchemaName SYSNAME, @TableName SYSNAME, @IndexName SYSNAME, @Frag FLOAT, @Pages BIGINT;
        DECLARE @ActionSql NVARCHAR(MAX);
        DECLARE @ActionsTaken INT = 0;

        DECLARE idx_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT s.name, t.name, i.name, ps.avg_fragmentation_in_percent, ps.page_count
            FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, ''LIMITED'') ps
            JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
            JOIN sys.tables t ON i.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE ps.page_count >= 1000
              AND ps.avg_fragmentation_in_percent >= 5
              AND i.name IS NOT NULL
              AND i.is_disabled = 0;

        OPEN idx_cursor;
        FETCH NEXT FROM idx_cursor INTO @SchemaName, @TableName, @IndexName, @Frag, @Pages;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF @Frag >= 30
                SET @ActionSql = N''ALTER INDEX '' + QUOTENAME(@IndexName) + N'' ON '' + QUOTENAME(@SchemaName) + N''.'' + QUOTENAME(@TableName) + N'' REBUILD;'';
            ELSE
                SET @ActionSql = N''ALTER INDEX '' + QUOTENAME(@IndexName) + N'' ON '' + QUOTENAME(@SchemaName) + N''.'' + QUOTENAME(@TableName) + N'' REORGANIZE;'';

            EXEC (@ActionSql);
            SET @ActionsTaken += 1;
            PRINT CONCAT(''  index maint: '', @SchemaName, ''.'', @TableName, ''.'', @IndexName, '' frag='', @Frag, ''% pages='', @Pages, '' -> '', @ActionSql);

            FETCH NEXT FROM idx_cursor INTO @SchemaName, @TableName, @IndexName, @Frag, @Pages;
        END
        CLOSE idx_cursor;
        DEALLOCATE idx_cursor;

        PRINT CONCAT(''INDEX_MAINT actions taken in '' + DB_NAME() + '': '', @ActionsTaken);
        ';

        BEGIN TRY
            EXEC (@Sql);
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'OK', Details = 'Index maintenance pass completed (threshold: >=30% rebuild, 5-30% reorganize, page_count>=1000).'
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('INDEX_MAINT OK: ', @DbName);
        END TRY
        BEGIN CATCH
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'FAILED', Details = ERROR_MESSAGE()
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('INDEX_MAINT FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
        END CATCH

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_UpdateStatistics
-- Purpose:   sp_updatestats against every live PlantSafety.* database.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_UpdateStatistics
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DbName SYSNAME;
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @Start DATETIME2(3);
    DECLARE @LogId BIGINT;

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
        SET @Start = SYSUTCDATETIME();
        INSERT INTO dbo.PlantSafety_MaintenanceLog (TaskType, DatabaseName, StartUtc, Result, Details)
        VALUES ('UPDATE_STATS', @DbName, @Start, 'OK', NULL);
        SET @LogId = SCOPE_IDENTITY();

        SET @Sql = N'USE ' + QUOTENAME(@DbName) + N'; EXEC sp_updatestats;';

        BEGIN TRY
            EXEC (@Sql);
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'OK', Details = 'sp_updatestats completed.'
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('UPDATE_STATS OK: ', @DbName);
        END TRY
        BEGIN CATCH
            UPDATE dbo.PlantSafety_MaintenanceLog
                SET EndUtc = SYSUTCDATETIME(), Result = 'FAILED', Details = ERROR_MESSAGE()
                WHERE MaintenanceLogId = @LogId;
            PRINT CONCAT('UPDATE_STATS FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
        END CATCH

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

-- -----------------------------------------------------------------------------
-- Agent job registration (idempotent: drop-if-exists, then recreate)
-- -----------------------------------------------------------------------------

-- 1) Integrity check -- weekly, Saturday 23:00
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Integrity Check (DBCC CHECKDB)')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Integrity Check (DBCC CHECKDB)';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Integrity Check (DBCC CHECKDB)',
    @enabled = 1,
    @description = N'Weekly DBCC CHECKDB (read-only, no repair) against every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_IntegrityCheck. Results logged to msdb.dbo.PlantSafety_MaintenanceLog.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Integrity Check',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_IntegrityCheck;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Weekly Saturday 23:00',
    @freq_type = 8,
    @freq_interval = 64,          -- Saturday
    @freq_recurrence_factor = 1,
    @active_start_time = 230000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Integrity Check (DBCC CHECKDB)',
    @schedule_name = N'PlantSafety - Weekly Saturday 23:00';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Integrity Check (DBCC CHECKDB)';
GO

-- 2) Index maintenance -- weekly, Saturday 23:30
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Index Maintenance (Rebuild-Reorganize)')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Index Maintenance (Rebuild-Reorganize)';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Index Maintenance (Rebuild-Reorganize)',
    @enabled = 1,
    @description = N'Weekly fragmentation-threshold index maintenance (REBUILD >=30%, REORGANIZE 5-30%) against every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_IndexMaintenance. Thresholds are placeholder Microsoft-conventional values pending governance confirmation.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Index Maintenance',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_IndexMaintenance;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Weekly Saturday 23:30',
    @freq_type = 8,
    @freq_interval = 64,          -- Saturday
    @freq_recurrence_factor = 1,
    @active_start_time = 233000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Index Maintenance (Rebuild-Reorganize)',
    @schedule_name = N'PlantSafety - Weekly Saturday 23:30';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Index Maintenance (Rebuild-Reorganize)';
GO

-- 3) Update statistics -- daily 02:00
DECLARE @JobId UNIQUEIDENTIFIER;
DECLARE @OwnerLogin SYSNAME = SUSER_SNAME();

IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - Update Statistics')
    EXEC msdb.dbo.sp_delete_job @job_name = N'PlantSafety - Update Statistics';

EXEC msdb.dbo.sp_add_job
    @job_name = N'PlantSafety - Update Statistics',
    @enabled = 1,
    @description = N'Daily sp_updatestats against every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_UpdateStatistics.',
    @category_name = N'Database Maintenance',
    @owner_login_name = @OwnerLogin,
    @job_id = @JobId OUTPUT;

EXEC msdb.dbo.sp_add_jobstep
    @job_id = @JobId,
    @step_name = N'Run Update Statistics',
    @subsystem = N'TSQL',
    @command = N'EXEC msdb.dbo.usp_PlantSafety_UpdateStatistics;',
    @database_name = N'msdb',
    @on_success_action = 1,
    @on_fail_action = 2,
    @retry_attempts = 1,
    @retry_interval = 5;

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = N'PlantSafety - Daily 02:00',
    @freq_type = 4,                -- daily
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = N'PlantSafety - Update Statistics',
    @schedule_name = N'PlantSafety - Daily 02:00';

EXEC msdb.dbo.sp_add_jobserver
    @job_name = N'PlantSafety - Update Statistics';
GO

PRINT 'Phase 12 / 121: maintenance procedures + Agent jobs registered.';
GO

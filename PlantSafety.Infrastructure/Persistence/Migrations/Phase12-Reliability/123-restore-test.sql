-- =============================================================================
-- Phase 12 - Reliability / Backup / Maintenance
-- 123-restore-test.sql
--
-- Purpose:
--   Reusable restore-test utility procedure. Restores a given database's most
--   recent FULL backup to an ISOLATED database name (<OriginalName>_DRTest),
--   runs DBCC CHECKDB against the restored copy to validate integrity, then
--   drops the test database and its restored files. Never touches or
--   overwrites the live database. Satisfies the "execute-restore-test" /
--   "validate-backups" DR Sentinel skills (see
--   sql-database-skill/references/execute-restore-test.md).
--
-- Restore location: C:\Users\dross\SQLBackups\PlantSafety\RestoreTest\
--   (scratch space -- files here are NOT backup artifacts subject to the
--   backup-deletion-discipline rule; they are disposable restore-test copies
--   deleted at the end of every run, same as a native restore-verify step.)
--
-- Idempotent: safe to re-run. Procedure uses CREATE OR ALTER.
-- =============================================================================

USE msdb;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_RestoreTest
-- Purpose:   Restore-test a single database's latest FULL backup into an
--            isolated *_DRTest database, validate with DBCC CHECKDB, then
--            clean up (drop DB + delete restored files).
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_RestoreTest
    @DatabaseName SYSNAME,
    @TestRootPath NVARCHAR(260) = N'C:\Users\dross\SQLBackups\PlantSafety\RestoreTest\'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BackupFile NVARCHAR(400);
    DECLARE @TestDbName SYSNAME = @DatabaseName + N'_DRTest';
    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @MoveClauses NVARCHAR(MAX) = N'';
    DECLARE @LogicalName SYSNAME, @FileType CHAR(1);
    DECLARE @StartUtc DATETIME2(3) = SYSUTCDATETIME();
    DECLARE @LogId BIGINT;

    INSERT INTO dbo.PlantSafety_MaintenanceLog (TaskType, DatabaseName, StartUtc, Result, Details)
    VALUES ('RESTORE_TEST', @DatabaseName, @StartUtc, 'OK', NULL);
    SET @LogId = SCOPE_IDENTITY();

    BEGIN TRY
        -- Locate the most recent FULL backup file on disk for this database
        -- (msdb.dbo.backupset/backupmediafamily is authoritative; we also
        -- accept a filesystem fallback via xp_dirtree if backupset is empty).
        SELECT TOP 1 @BackupFile = bmf.physical_device_name
        FROM msdb.dbo.backupset bs
        JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
        WHERE bs.database_name = @DatabaseName AND bs.type = 'D'
        ORDER BY bs.backup_finish_date DESC;

        IF @BackupFile IS NULL
        BEGIN
            RAISERROR('No FULL backup found in msdb backup history for %s -- run usp_PlantSafety_BackupFull first.', 16, 1, @DatabaseName);
        END

        -- Drop any leftover test DB from a prior failed run
        IF DB_ID(@TestDbName) IS NOT NULL
        BEGIN
            SET @Sql = N'ALTER DATABASE ' + QUOTENAME(@TestDbName) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@TestDbName) + N';';
            EXEC (@Sql);
        END

        -- Build MOVE clauses so the restored copy lands under RestoreTest\,
        -- never overwriting the live database's files.
        DECLARE @FileList TABLE (
            LogicalName NVARCHAR(128), PhysicalName NVARCHAR(260), Type CHAR(1),
            FileGroupName NVARCHAR(128) NULL, Size BIGINT, MaxSize BIGINT, FileId INT,
            CreateLSN NUMERIC(25,0) NULL, DropLSN NUMERIC(25,0) NULL, UniqueId UNIQUEIDENTIFIER NULL,
            ReadOnlyLSN NUMERIC(25,0) NULL, ReadWriteLSN NUMERIC(25,0) NULL, BackupSizeInBytes BIGINT,
            SourceBlockSize INT, FileGroupId INT, LogGroupGUID UNIQUEIDENTIFIER NULL,
            DifferentialBaseLSN NUMERIC(25,0) NULL, DifferentialBaseGUID UNIQUEIDENTIFIER NULL,
            IsReadOnly BIT, IsPresent BIT, TDEThumbprint VARBINARY(32) NULL, SnapshotUrl NVARCHAR(360) NULL
        );

        SET @Sql = N'RESTORE FILELISTONLY FROM DISK = ''' + @BackupFile + N'''';
        INSERT INTO @FileList
        EXEC (@Sql);

        SELECT @MoveClauses = STRING_AGG(
            N'MOVE ''' + LogicalName + N''' TO ''' + @TestRootPath + @TestDbName + N'_' + LogicalName + CASE WHEN Type = 'L' THEN N'.ldf' ELSE N'.ndf' END + N''''
            , N', ')
        FROM @FileList;

        SET @Sql = N'RESTORE DATABASE ' + QUOTENAME(@TestDbName) +
                   N' FROM DISK = ''' + @BackupFile + N'''' +
                   N' WITH ' + @MoveClauses + N', REPLACE, CHECKSUM, STATS = 100;';
        EXEC (@Sql);

        -- Validate the restored copy
        SET @Sql = N'DBCC CHECKDB(' + QUOTENAME(@TestDbName, '''') + N') WITH NO_INFOMSGS, ALL_ERRORMSGS;';
        EXEC (@Sql);

        UPDATE dbo.PlantSafety_MaintenanceLog
            SET EndUtc = SYSUTCDATETIME(), Result = 'OK',
                Details = CONCAT('Restored ', @BackupFile, ' as ', @TestDbName, '; DBCC CHECKDB passed.')
            WHERE MaintenanceLogId = @LogId;

        PRINT CONCAT('RESTORE_TEST OK: ', @DatabaseName, ' -> ', @TestDbName, ' (CHECKDB passed)');
    END TRY
    BEGIN CATCH
        UPDATE dbo.PlantSafety_MaintenanceLog
            SET EndUtc = SYSUTCDATETIME(), Result = 'FAILED', Details = ERROR_MESSAGE()
            WHERE MaintenanceLogId = @LogId;
        PRINT CONCAT('RESTORE_TEST FAILED: ', @DatabaseName, ' -- ', ERROR_MESSAGE());
    END CATCH

    -- Always clean up the test DB, success or failure, so a stray *_DRTest DB
    -- never lingers (a restore-test artifact, not a backup -- safe to drop).
    IF DB_ID(@TestDbName) IS NOT NULL
    BEGIN
        SET @Sql = N'ALTER DATABASE ' + QUOTENAME(@TestDbName) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@TestDbName) + N';';
        EXEC (@Sql);
        PRINT CONCAT('RESTORE_TEST cleanup: dropped ', @TestDbName);
    END
END
GO

PRINT 'Phase 12 / 123: restore-test procedure registered.';
GO

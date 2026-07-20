-- =============================================================================
-- Phase 18 - Express Checklist (SQL Server Agent Disabled Remediation)
-- 180-database-size-log.sql
--
-- Purpose:
--   Genuine gap identified in the Express-specific DR checklist: no database
--   size monitoring existed anywhere in Phases 1-17. This migration creates
--   a lightweight snapshot table + capture procedure, mirroring the existing
--   msdb.dbo.PlantSafety_MaintenanceLog pattern from Phase 12
--   (121-maintenance-jobs-integrity-index-stats.sql), so it lives alongside
--   the other cross-database platform-utility objects in msdb.
--
-- Scope: same dynamic inventory approach as Phase 12/120 and 121
--   (sys.databases WHERE name LIKE N'PlantSafety.%' AND state_desc = 'ONLINE'
--   AND database_id > 4) -- live-verified 2026-07-19: 18 PlantSafety.*
--   databases on this instance.
--
-- Data source: sys.master_files (per-file size/used-space, the DMV-based
--   equivalent of sp_spaceused that works across every database from a
--   single query without needing to iterate USE statements per DB), joined
--   with FILEPROPERTY(..., 'SpaceUsed') evaluated per-database via dynamic
--   SQL (FILEPROPERTY only resolves against the CURRENT database context,
--   so a per-database EXEC is required for the used-space column; sizes
--   themselves come from sys.master_files directly at the server level).
--
-- Idempotent: safe to re-run. Table create is existence-checked. Procedure
-- uses CREATE OR ALTER.
-- =============================================================================

USE msdb;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantSafety_DatabaseSizeLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PlantSafety_DatabaseSizeLog
    (
        DatabaseSizeLogId  BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CapturedUtc        DATETIME2(3)  NOT NULL,
        DatabaseName       SYSNAME       NOT NULL,
        FileLogicalName    SYSNAME       NOT NULL,
        FileType           VARCHAR(10)   NOT NULL,    -- 'ROWS' | 'LOG'
        SizeMB             DECIMAL(18,2) NOT NULL,     -- allocated file size
        UsedSpaceMB        DECIMAL(18,2) NULL,          -- actual used space (NULL if FILEPROPERTY lookup failed)
        PhysicalName       NVARCHAR(400) NOT NULL,
        Result             VARCHAR(20)   NOT NULL,    -- 'OK' | 'FAILED'
        Details            NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_PlantSafety_DatabaseSizeLog_DatabaseName_CapturedUtc
        ON dbo.PlantSafety_DatabaseSizeLog (DatabaseName, CapturedUtc DESC);
END
GO

-- =============================================
-- Procedure: msdb.dbo.usp_PlantSafety_LogDatabaseSizes
-- Purpose:   Snapshot data/log file size + used space for every live
--            PlantSafety.* database into dbo.PlantSafety_DatabaseSizeLog.
--            Read-only / safe. One row per file per run.
-- =============================================
CREATE OR ALTER PROCEDURE dbo.usp_PlantSafety_LogDatabaseSizes
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Captured DATETIME2(3) = SYSUTCDATETIME();
    DECLARE @DbName SYSNAME;
    DECLARE @Sql NVARCHAR(MAX);

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
        BEGIN TRY
            SET @Sql = N'
                USE ' + QUOTENAME(@DbName) + N';
                INSERT INTO msdb.dbo.PlantSafety_DatabaseSizeLog
                    (CapturedUtc, DatabaseName, FileLogicalName, FileType, SizeMB, UsedSpaceMB, PhysicalName, Result, Details)
                SELECT
                    @CapturedUtc,
                    DB_NAME(),
                    mf.name,
                    CASE mf.type_desc WHEN ''LOG'' THEN ''LOG'' ELSE ''ROWS'' END,
                    CAST(mf.size / 128.0 AS DECIMAL(18,2)),
                    CAST(CAST(FILEPROPERTY(mf.name, ''SpaceUsed'') AS BIGINT) / 128.0 AS DECIMAL(18,2)),
                    mf.physical_name,
                    ''OK'',
                    NULL
                FROM sys.master_files mf
                WHERE mf.database_id = DB_ID();';

            EXEC sp_executesql @Sql, N'@CapturedUtc DATETIME2(3)', @CapturedUtc = @Captured;

            PRINT CONCAT('Database size snapshot OK: ', @DbName);
        END TRY
        BEGIN CATCH
            INSERT INTO msdb.dbo.PlantSafety_DatabaseSizeLog
                (CapturedUtc, DatabaseName, FileLogicalName, FileType, SizeMB, UsedSpaceMB, PhysicalName, Result, Details)
            VALUES
                (@Captured, @DbName, N'(unknown)', N'ROWS', 0, NULL, N'(unknown)', 'FAILED', ERROR_MESSAGE());

            PRINT CONCAT('Database size snapshot FAILED: ', @DbName, ' -- ', ERROR_MESSAGE());
        END CATCH

        FETCH NEXT FROM db_cursor INTO @DbName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;
END
GO

PRINT 'Phase 18 / 180: database size log table + capture procedure registered.';
GO

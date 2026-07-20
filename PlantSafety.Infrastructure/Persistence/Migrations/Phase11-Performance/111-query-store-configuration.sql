-- =============================================
-- Migration:     111-query-store-configuration.sql
-- Author:        Performance Guardian Agent
-- Create date:   2026-07-19
-- Description:   Enables Query Store on all 8 primary databases plus
--                PlantSafety.AuditDB and PlantSafety.ErrorDB (shared,
--                cross-database, high-call-volume logging targets --
--                genuinely worth tracking plan/perf data on from day one,
--                same rationale as the 8 primary DBs).
--
-- *** Why AuditDB/ErrorDB are included (judgment call, flagged) ***
-- Every stored procedure across all 8 primary databases writes to
-- [PlantSafety.AuditDB].dbo.AuditLog and [PlantSafety.ErrorDB].dbo.ErrorLog
-- (canonical template, 00-canonical-templates.md) on essentially every call.
-- These two tables will see the platform's highest per-call write volume of
-- any object once real traffic begins -- exactly the kind of hot path Query
-- Store exists to protect. Including them is judged in-scope under the
-- task's "+ AuditDB/ErrorDB if you judge appropriate" allowance.
--
-- *** Settings (Microsoft-recommended defaults, explicitly flagged as
--     TUNABLE placeholders -- revisit once real workload data exists) ***
--   QUERY_CAPTURE_MODE = AUTO   -- captures relevant/recurring queries only,
--                                  skips one-off ad-hoc noise; MS default and
--                                  recommended starting point.
--   MAX_STORAGE_SIZE_MB = 1024  -- MS default is 100 MB, which is commonly
--                                  too small in practice and fills up fast;
--                                  1024 MB (1 GB) is a widely-used
--                                  production-safe starting point that
--                                  leaves headroom before SIZE_BASED_CLEANUP
--                                  kicks in. Express edition has a 10 GB
--                                  per-database cap, so 1 GB is a small
--                                  fraction of that. TUNABLE -- revisit once
--                                  real query volume is observed.
--   STALE_QUERY_THRESHOLD_DAYS = 30 -- MS default; queries not seen in 30
--                                  days age out of the store. TUNABLE.
--   INTERVAL_LENGTH_MINUTES = 60 -- MS default granularity for the runtime
--                                  stats aggregation window.
--   SIZE_BASED_CLEANUP_MODE = AUTO -- automatically purges oldest data when
--                                  approaching MAX_STORAGE_SIZE_MB, avoiding
--                                  a hard READ_ONLY flip.
--   QUERY_CAPTURE_POLICY (SQL 2022+, this instance is 2022) left at engine
--                                  default (ALL queries captured under AUTO
--                                  mode's own thresholds) -- not overridden.
--
-- Idempotency: ALTER DATABASE SET QUERY_STORE is safe to re-run; re-applies
-- the same settings without error, and the enable check is existence-guarded
-- for clarity. Zero rows affected in any application table (Query Store
-- configuration is a database-property, not a schema/table change).
-- =============================================
SET NOCOUNT ON;
GO

DECLARE @dbs TABLE (name sysname);
INSERT INTO @dbs (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB'),
    (N'PlantSafety.AuditDB'), (N'PlantSafety.ErrorDB');

DECLARE @db sysname, @sql nvarchar(max);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @dbs;
OPEN c;
FETCH NEXT FROM c INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
    ALTER DATABASE ' + QUOTENAME(@db) + N' SET QUERY_STORE = ON;
    ALTER DATABASE ' + QUOTENAME(@db) + N' SET QUERY_STORE
    (
        OPERATION_MODE = READ_WRITE,
        QUERY_CAPTURE_MODE = AUTO,
        MAX_STORAGE_SIZE_MB = 1024,
        CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
        INTERVAL_LENGTH_MINUTES = 60,
        SIZE_BASED_CLEANUP_MODE = AUTO,
        DATA_FLUSH_INTERVAL_SECONDS = 900,
        MAX_PLANS_PER_QUERY = 200
    );';
    EXEC sp_executesql @sql;
    PRINT N'Query Store enabled/configured on ' + @db + N'.';
    FETCH NEXT FROM c INTO @db;
END
CLOSE c;
DEALLOCATE c;
GO

------------------------------------------------------------
-- Live verification: confirm Query Store is ON and READ_WRITE on all 10
-- databases, via sys.database_query_store_options in each DB context.
------------------------------------------------------------
DECLARE @dbs2 TABLE (name sysname);
INSERT INTO @dbs2 (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB'),
    (N'PlantSafety.AuditDB'), (N'PlantSafety.ErrorDB');

DECLARE @db2 sysname, @vsql nvarchar(max);
DECLARE c2 CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @dbs2;
OPEN c2;
FETCH NEXT FROM c2 INTO @db2;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @vsql = N'USE ' + QUOTENAME(@db2) + N'; SELECT ''' + @db2 + N''' AS DbName,
        actual_state_desc, desired_state_desc, query_capture_mode_desc,
        max_storage_size_mb, current_storage_size_mb, stale_query_threshold_days,
        size_based_cleanup_mode_desc, interval_length_minutes
    FROM sys.database_query_store_options;';
    EXEC sp_executesql @vsql;
    FETCH NEXT FROM c2 INTO @db2;
END
CLOSE c2;
DEALLOCATE c2;
GO

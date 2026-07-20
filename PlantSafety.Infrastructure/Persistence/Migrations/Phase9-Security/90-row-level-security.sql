-- =============================================
-- Migration:     90-row-level-security.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Row-Level Security (RLS) tenant-isolation predicate,
--                applied as a FILTER + BLOCK security policy to every
--                genuinely tenant-scoped business table (uniqueidentifier
--                NOT NULL TenantId column) across the multi-tenant
--                databases: PlantSafety.CoreDB, PlantSafety.ArchiveDB,
--                PlantSafety.DiagnosticsDB.
--
-- *** Session-context convention -- ASSUMPTION, explicitly flagged ***
-- A full repo-wide search (grep -r "SESSION_CONTEXT" across all *.sql and
-- *.cs under C:\Users\dross\source) returned ZERO hits. There is no
-- established app-layer convention for setting SESSION_CONTEXT today.
-- Confirmed instead: every Dapper repository (e.g.
-- PlantSafety.Infrastructure\Analytics\Culture\Behavior\Persistence\
-- BehaviorDapperRepository.cs) passes TenantId as an explicit SP parameter
-- (`.AddGuid("TenantId", tenantId)`), not via session context.
--
-- Per the task's governance instruction ("apply the stricter compliance
-- rule and note the assumption; never loosen a control to make a check
-- pass"), this predicate is built as a REAL, strict, deny-by-default
-- SESSION_CONTEXT-based filter with NO bypass for app_executor/dbo. This
-- is a deliberate choice: a bypass for the app role would make the control
-- toothless for the exact traffic it exists to protect.
--
-- CONSEQUENCE (intentional, documented, not silently absorbed): because no
-- caller currently sets SESSION_CONTEXT('TenantId'), every SELECT/UPDATE/
-- DELETE against these tables from ANY current connection (including the
-- existing dev workflow and the app's own stored procedures) will see ZERO
-- rows, and every INSERT/UPDATE will be BLOCKED, until the application
-- connection layer is updated to call:
--     EXEC sp_set_session_context @key = N'TenantId', @value = @TenantId;
-- immediately after opening each connection/before invoking SPs, using the
-- same TenantId value already being passed as an explicit SP parameter
-- today. This is a required follow-up C# change to the Dapper connection
-- factory / repository base class -- OUT OF SCOPE for this SQL-only agent
-- (SQL migration scope only; no C# changes made here). Flagging rather than
-- silently building a weaker, pass-through predicate.
--
-- Safe to ship now: every genuinely tenant-scoped business table in scope
-- is confirmed at 0 rows (live-verified via sys.dm_db_partition_stats) --
-- there is no live traffic to break at deployment time; the landmine is
-- for the next INSERT, which is why the follow-up above is called out
-- explicitly rather than left implicit. (One exception, discovered and
-- excluded below: ArchiveDB.Archival.ArchivalJob had 5 pre-existing rows
-- from Phase 7's retention-job stored procedures, all with the all-zeros
-- system-sentinel TenantId -- see the ArchiveDB scope note further down.
-- Confirmed these rows were not disturbed by this migration.)
--
-- Scope decision -- ArchiveDB.Archival.ArchivalJob EXCLUDED (discovered
-- live during this migration's verification pass, not assumed up front):
-- it has a NOT NULL uniqueidentifier TenantId column, so it was initially
-- picked up by this script's live-discovery query, but on inspection its 5
-- existing rows are ALL system-generated job-execution log entries from
-- Phase 7's scheduled retention/archival stored procedures
-- (74-retention-archival.sql), every one stamped with the all-zeros
-- sentinel TenantId (00000000-0000-0000-0000-000000000000) -- i.e. "this
-- job ran platform-wide," not tenant business data. This is the same
-- category as AuditDB.AuditLog/ErrorDB.ErrorLog (shared, cross-tenant
-- platform-operations logging), so it is excluded from the FILTER/BLOCK
-- policy for the same reason, and the exclusion is applied by explicit
-- table name below (not silently -- flagged here and in the ArchiveDB
-- block). Consequence: ArchiveDB currently has ZERO genuinely tenant-
-- scoped business tables, so no security policy is created there today;
-- the discovery query still runs each re-run so a real tenant-scoped
-- ArchiveDB table would be picked up automatically the moment one exists.
--
-- Scope decision -- AuditDB.dbo.AuditLog / ErrorDB.dbo.ErrorLog EXCLUDED:
-- both have a TenantId column, but it is nvarchar/NULLABLE (metadata about
-- which tenant an event pertains to), and both tables are the platform's
-- shared, cross-database, cross-module logging entry points (see the
-- approved usp_/TR_ naming exception in 00-canonical-templates.md, written
-- by usp_WriteAuditLog/usp_WriteErrorLog from every module). Applying a
-- BLOCK predicate here would risk breaking that shared write path platform
-- -wide for zero session-context benefit today, and audit/error logs are
-- legitimately platform-operations-visible artifacts, not siloed tenant
-- business data. Deliberately out of scope -- not a loosened control, a
-- correctly-scoped one; revisit if/when these tables gain direct tenant-
-- facing read exposure.
--
-- Idempotency: predicate function uses CREATE OR ALTER; security policy is
-- dropped and rebuilt from a live discovery of TenantId-bearing tables each
-- run, so this script is safe to re-run and self-heals against schema
-- drift (new tenant-scoped tables get picked up automatically next run).
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- PlantSafety.CoreDB
------------------------------------------------------------
USE [PlantSafety.CoreDB];
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
    EXEC('CREATE SCHEMA Security AUTHORIZATION dbo;');
GO

-- Drop the policy BEFORE (re)creating the predicate function -- a
-- SCHEMABINDING function cannot be ALTERed while a security policy still
-- references it. Rebuilt further down from a fresh live discovery of
-- TenantId-bearing tables, so this is safe and idempotent.
IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantAccessPolicy' AND schema_id = SCHEMA_ID('Security'))
    DROP SECURITY POLICY Security.TenantAccessPolicy;
GO

CREATE OR ALTER FUNCTION Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_accessResult
WHERE @TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
GO

DECLARE @sql nvarchar(max) = N'CREATE SECURITY POLICY Security.TenantAccessPolicy' + CHAR(13);
DECLARE @first bit = 1;
DECLARE @schemaName sysname, @tableName sysname;

DECLARE tblcur CURSOR LOCAL FAST_FORWARD FOR
    SELECT s.name, t.name
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    JOIN sys.types ty ON c.user_type_id = ty.user_type_id
    WHERE c.name = 'TenantId' AND ty.name = 'uniqueidentifier' AND c.is_nullable = 0
      AND s.name <> 'Security'
    ORDER BY s.name, t.name;

OPEN tblcur;
FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @first = 0 SET @sql = @sql + N',' + CHAR(13);
    SET @first = 0;
    SET @sql = @sql
        + N'ADD FILTER PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N',' + CHAR(13)
        + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER INSERT,' + CHAR(13)
        + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER UPDATE';
    FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
END
CLOSE tblcur;
DEALLOCATE tblcur;

SET @sql = @sql + CHAR(13) + N'WITH (STATE = ON);';
EXEC sp_executesql @sql;
GO

------------------------------------------------------------
-- PlantSafety.ArchiveDB
------------------------------------------------------------
USE [PlantSafety.ArchiveDB];
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
    EXEC('CREATE SCHEMA Security AUTHORIZATION dbo;');
GO

IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantAccessPolicy' AND schema_id = SCHEMA_ID('Security'))
    DROP SECURITY POLICY Security.TenantAccessPolicy;
GO

CREATE OR ALTER FUNCTION Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_accessResult
WHERE @TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
GO

-- NOTE: Archival.ArchivalJob is deliberately excluded here (AND NOT (...)
-- below) -- see the header note above. Its rows are system-generated
-- retention-job execution logs stamped with the all-zeros sentinel
-- TenantId, not tenant business data.
DECLARE @sql nvarchar(max) = N'CREATE SECURITY POLICY Security.TenantAccessPolicy' + CHAR(13);
DECLARE @first bit = 1;
DECLARE @schemaName sysname, @tableName sysname;
DECLARE @tableCount int;

DECLARE tblcur CURSOR LOCAL FAST_FORWARD FOR
    SELECT s.name, t.name
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    JOIN sys.types ty ON c.user_type_id = ty.user_type_id
    WHERE c.name = 'TenantId' AND ty.name = 'uniqueidentifier' AND c.is_nullable = 0
      AND s.name <> 'Security'
      AND NOT (s.name = 'Archival' AND t.name = 'ArchivalJob')
    ORDER BY s.name, t.name;

SELECT @tableCount = COUNT(*)
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE c.name = 'TenantId' AND ty.name = 'uniqueidentifier' AND c.is_nullable = 0
  AND s.name <> 'Security'
  AND NOT (s.name = 'Archival' AND t.name = 'ArchivalJob');

IF @tableCount = 0
    PRINT 'PlantSafety.ArchiveDB: no genuinely tenant-scoped business tables found (ArchivalJob excluded as system-level job log) -- no security policy created. Re-run this script once a real tenant-scoped ArchiveDB table exists.';
ELSE
BEGIN
    OPEN tblcur;
    FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @first = 0 SET @sql = @sql + N',' + CHAR(13);
        SET @first = 0;
        SET @sql = @sql
            + N'ADD FILTER PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
            + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N',' + CHAR(13)
            + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
            + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER INSERT,' + CHAR(13)
            + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
            + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER UPDATE';
        FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
    END
    CLOSE tblcur;
    DEALLOCATE tblcur;

    SET @sql = @sql + CHAR(13) + N'WITH (STATE = ON);';
    EXEC sp_executesql @sql;
END
GO

------------------------------------------------------------
-- PlantSafety.DiagnosticsDB
------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB];
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
    EXEC('CREATE SCHEMA Security AUTHORIZATION dbo;');
GO

IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'TenantAccessPolicy' AND schema_id = SCHEMA_ID('Security'))
    DROP SECURITY POLICY Security.TenantAccessPolicy;
GO

CREATE OR ALTER FUNCTION Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_accessResult
WHERE @TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
GO

DECLARE @sql nvarchar(max) = N'CREATE SECURITY POLICY Security.TenantAccessPolicy' + CHAR(13);
DECLARE @first bit = 1;
DECLARE @schemaName sysname, @tableName sysname;

DECLARE tblcur CURSOR LOCAL FAST_FORWARD FOR
    SELECT s.name, t.name
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    JOIN sys.types ty ON c.user_type_id = ty.user_type_id
    WHERE c.name = 'TenantId' AND ty.name = 'uniqueidentifier' AND c.is_nullable = 0
      AND s.name <> 'Security'
    ORDER BY s.name, t.name;

OPEN tblcur;
FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @first = 0 SET @sql = @sql + N',' + CHAR(13);
    SET @first = 0;
    SET @sql = @sql
        + N'ADD FILTER PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N',' + CHAR(13)
        + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER INSERT,' + CHAR(13)
        + N'ADD BLOCK PREDICATE Security.fn_TenantAccessPredicate(TenantId) ON '
        + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) + N' AFTER UPDATE';
    FETCH NEXT FROM tblcur INTO @schemaName, @tableName;
END
CLOSE tblcur;
DEALLOCATE tblcur;

SET @sql = @sql + CHAR(13) + N'WITH (STATE = ON);';
EXEC sp_executesql @sql;
GO

------------------------------------------------------------
-- Verification: policy + predicate counts per database
------------------------------------------------------------
SELECT 'PlantSafety.CoreDB' AS DatabaseName, sp.name AS PolicyName, sp.is_enabled,
    (SELECT COUNT(*) FROM [PlantSafety.CoreDB].sys.security_predicates) AS PredicateCount
FROM [PlantSafety.CoreDB].sys.security_policies sp;
SELECT 'PlantSafety.ArchiveDB' AS DatabaseName, sp.name AS PolicyName, sp.is_enabled,
    (SELECT COUNT(*) FROM [PlantSafety.ArchiveDB].sys.security_predicates) AS PredicateCount
FROM [PlantSafety.ArchiveDB].sys.security_policies sp;
SELECT 'PlantSafety.DiagnosticsDB' AS DatabaseName, sp.name AS PolicyName, sp.is_enabled,
    (SELECT COUNT(*) FROM [PlantSafety.DiagnosticsDB].sys.security_predicates) AS PredicateCount
FROM [PlantSafety.DiagnosticsDB].sys.security_policies sp;
GO

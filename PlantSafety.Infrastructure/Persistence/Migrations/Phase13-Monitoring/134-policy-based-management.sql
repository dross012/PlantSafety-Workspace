-- =============================================
-- Phase 13 - Monitoring, Diagnostics, Governance Tooling
-- Script:        134-policy-based-management.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Purpose:       Documents the Policy-Based Management (PBM) work done this session and
--                provides the on-demand T-SQL governance-check equivalent for the one PBM
--                policy that was assessed as high-effort-for-low-value and skipped.
--
-- What was actually deployed (via SMO/DMF PowerShell, since msdb PBM objects are XML-
-- serialized objects most reliably authored through the SMO Policy-Based Management API,
-- not raw INSERTs into syspolicy_* tables):
--   - Policy category:  PlantSafety_Governance
--   - Condition/Policy: PlantSafety_TableHasPrimaryKey / PlantSafety_TableHasPrimaryKey_Policy
--       Facet = Table, checks HasPrimaryClusteredIndex = true.
--       AutomatedPolicyEvaluationMode = None (on-demand only -- see note below on why).
--       LIVE-VERIFIED 2026-07-19: evaluated per-table against all 61 real (non-system)
--       tables in PlantSafety.CoreDB -> 61/61 True, matching an independent direct T-SQL
--       scan of sys.key_constraints (0 tables missing a PK). Also cross-checked via plain
--       T-SQL across all 8 primary databases: 0 missing PKs in 7 of 8 databases; 1 finding
--       in PlantSafety.IntegrationDB -- see note below (expected, not a real violation).
--     Known limitation (documented, not fixed, per "light touch" scope): evaluating this
--     policy against a whole Database object directly (rather than iterating its Tables
--     collection) returns a misleading False, because no TargetSet was configured to teach
--     PBM how to traverse Database -> Table automatically for ad-hoc evaluation. Building
--     that TargetSet correctly is additional SMO-object-model effort beyond "light touch."
--     Until a TargetSet is added, evaluate this policy per-table (or against the databases
--     via the direct T-SQL query at the bottom of this file, which is exactly equivalent
--     and was used to independently verify the policy's Table-level logic above).
--
-- What was attempted and explicitly DROPPED (do not recreate without fixing the root cause):
--   - Condition/Policy: PlantSafety_NoSelectStarInProcs / PlantSafety_NoSelectStarInProcs_Policy
--       Facet = StoredProcedure, intended to use an ExecuteSql() function node to inspect
--       OBJECT_DEFINITION() for 'SELECT *'. LIVE-TESTED 2026-07-19: the @Schema/@Name
--       token substitution inside the ExecuteSql query text did not bind the way assumed --
--       the policy returned False (non-compliant) for every single stored procedure in
--       PlantSafety.CoreDB, including ones independently confirmed via direct T-SQL to
--       contain no 'SELECT *' anywhere. Rather than ship a governance control that always
--       fails (which would be worse than no control -- alert fatigue / trained-to-ignore),
--       it was dropped. The equivalent check is fully expressible and reliable as plain
--       T-SQL (see the query below) and is the recommended way to run this check until/
--       unless a human operator wants to invest the additional effort to get the PBM
--       ExecuteSql binding syntax correct (SSMS's policy designer generates this correctly
--       via its UI; hand-authoring the exact expression-tree token format via SMO proved
--       non-trivial and is exactly the kind of thing this task's "skip and document why"
--       escape hatch exists for).
--
-- Why AutomatedPolicyEvaluationMode = None (on-demand only) for the policy that IS kept:
--   SQL Server Agent (SQLAgent$SQLEXPRESS) is present but STOPPED on this Express-edition
--   instance -- confirmed live via xp_servicecontrol 'QUERYSTATE'. PBM's "On Schedule" and
--   "On Change: Log Only" automation modes both depend on SQL Agent jobs/DDL triggers tied
--   to Agent; "On Change: Prevent" additionally requires enabling the server-level DDL
--   trigger PBM manages itself, which was deliberately not enabled given this session's
--   separate, unrelated finding that DAC (the safety net for exactly this kind of
--   trigger-based lockout risk) is unavailable in this environment (see script 133's
--   header for the full DAC investigation). On-demand evaluation (CheckOnDemand, run
--   manually or via a future Agent-enabled environment) is the only mode exercised here.
--
-- IntegrationDB finding note: the single table without a PK in PlantSafety.IntegrationDB
-- is `dbo.MSchange_tracking_history`, a SQL Server Change Tracking SYSTEM-GENERATED table
-- created when Change Tracking was enabled on PlantSafety.IntegrationDB in Phase 10
-- (`101-change-tracking-integrationdb.sql`). It is a heap by SQL Server's own design, not a
-- PlantSafety schema-owned entity, and is NOT a real governance violation -- documented
-- here rather than "fixed" so this expected exception isn't mistaken for a regression on
-- the next policy re-run.
--
-- This script itself is a documentation/reference artifact (the PBM objects were created
-- live via PowerShell/SMO, not via this .sql file -- msdb PBM storage is XML-serialized and
-- not reliably hand-authored as raw INSERTs). Re-running the PowerShell is the correct way
-- to reproduce; this file records exactly what was run and why, per migration-tracking
-- discipline, and provides the T-SQL equivalent for both checks below.
-- =============================================

------------------------------------------------------------
-- On-demand T-SQL equivalent: "tables must have a primary key"
-- (used to independently verify the PBM policy above; safe to run anytime, read-only)
------------------------------------------------------------
/*
DECLARE @dbs TABLE (name sysname);
INSERT INTO @dbs VALUES
 ('PlantSafety.ArchiveDB'),('PlantSafety.CloudIdentityDB'),('PlantSafety.CoreDB'),
 ('PlantSafety.DiagnosticsDB'),('PlantSafety.ExportImportDB'),('PlantSafety.IntegrationDB'),
 ('PlantSafety.PredictiveDB'),('PlantSafety.ReportingDB');
DECLARE @db sysname, @sql nvarchar(max) = N'';
DECLARE c CURSOR FOR SELECT name FROM @dbs;
OPEN c; FETCH NEXT FROM c INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = @sql + N'
SELECT ''' + @db + N''' AS DbName, s.name AS SchemaName, t.name AS TableName
FROM [' + @db + N'].sys.tables t
JOIN [' + @db + N'].sys.schemas s ON t.schema_id = s.schema_id
WHERE NOT EXISTS (SELECT 1 FROM [' + @db + N'].sys.key_constraints kc
                   WHERE kc.parent_object_id = t.object_id AND kc.type = ''PK'');';
    FETCH NEXT FROM c INTO @db;
END
CLOSE c; DEALLOCATE c;
EXEC sp_executesql @sql;
*/

------------------------------------------------------------
-- On-demand T-SQL equivalent: "stored procedures must not use SELECT *"
-- (the reliable replacement for the dropped PlantSafety_NoSelectStarInProcs_Policy)
------------------------------------------------------------
/*
DECLARE @dbs TABLE (name sysname);
INSERT INTO @dbs VALUES
 ('PlantSafety.ArchiveDB'),('PlantSafety.CloudIdentityDB'),('PlantSafety.CoreDB'),
 ('PlantSafety.DiagnosticsDB'),('PlantSafety.ExportImportDB'),('PlantSafety.IntegrationDB'),
 ('PlantSafety.PredictiveDB'),('PlantSafety.ReportingDB');
DECLARE @db sysname, @sql nvarchar(max) = N'';
DECLARE c CURSOR FOR SELECT name FROM @dbs;
OPEN c; FETCH NEXT FROM c INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = @sql + N'
SELECT ''' + @db + N''' AS DbName, s.name AS SchemaName, p.name AS ProcName
FROM [' + @db + N'].sys.procedures p
JOIN [' + @db + N'].sys.schemas s ON p.schema_id = s.schema_id
WHERE ISNULL(OBJECT_DEFINITION(p.object_id), N'''') LIKE N''%SELECT *%'';';
    FETCH NEXT FROM c INTO @db;
END
CLOSE c; DEALLOCATE c;
EXEC sp_executesql @sql;
*/

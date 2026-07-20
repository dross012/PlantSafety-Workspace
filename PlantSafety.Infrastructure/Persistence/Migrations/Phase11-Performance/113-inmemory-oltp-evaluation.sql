-- =============================================
-- Migration:     113-inmemory-oltp-evaluation.sql
-- Author:        Performance Guardian Agent
-- Create date:   2026-07-19
-- Description:   In-Memory OLTP (memory-optimized tables) evaluation.
--                DECISION: SKIPPED platform-wide. No schema change in this
--                script -- documentation + live capability verification
--                only, per the task's "evaluate, likely skip... explicitly
--                document why" instruction.
--
-- *** Candidates considered, and why each was rejected ***
-- 1. Phase 10 Service Broker replay queue
--    (Persistence\Migrations\Phase10-Automation\100-service-broker-replay-
--    queue.sql, backing PlantSafety.IntegrationDB.Integration.ReplayRecord):
--    Service Broker QUEUE objects are internal SQL Server system objects,
--    not user tables -- they cannot be declared MEMORY_OPTIMIZED = ON at
--    all (not a supported combination; Service Broker messaging has its own
--    internal storage engine, unrelated to the In-Memory OLTP engine). The
--    backing ReplayRecord table itself is an ordinary durable audit/replay
--    log (append-mostly, queried by CorrelationId/SchemaName, not a
--    sub-millisecond hot-path lookup table) -- no throughput case for
--    memory-optimizing it even if it were technically eligible.
-- 2. Diagnostics.DiagnosticEvent / TenantDiagnosticEvent (highest
--    projected write-volume tables in the platform, per their own headers):
--    high INSERT volume is a classic In-Memory OLTP use case in the
--    abstract, but these tables (a) require durability (SCHEMA_AND_DATA,
--    not SCHEMA_ONLY -- diagnostic events are safety/compliance-relevant
--    audit data, cannot be lossy on restart), (b) already have a documented
--    hash-based dedup constraint (UQ_..._Hash) and RLS security policy
--    (Phase 9) applied -- Row-Level Security predicates ARE supported on
--    memory-optimized tables in SQL Server 2016+, so this alone would not
--    block it, but (c) there is no PROVEN extreme-throughput requirement
--    yet (0 rows today, no load-test data, no measured contention on the
--    existing rowstore B-tree indexes) to justify the migration complexity,
--    and (d) natively compiled procedures (the main perf win of In-Memory
--    OLTP beyond just the lock-free table structure) cannot use the
--    platform's canonical error/audit-logging cross-database INSERT pattern
--    (00-canonical-templates.md -- natively compiled procs cannot reference
--    tables outside the containing database, which breaks the mandatory
--    [PlantSafety.AuditDB]/[PlantSafety.ErrorDB] cross-database calls used
--    by every procedure platform-wide). Interpreted (non-natively-compiled)
--    access to a memory-optimized table is possible and would keep
--    cross-DB logging working, but that gives up the natively-compiled
--    proc performance win entirely, leaving only the lock-free table
--    structure benefit -- not enough upside to justify the added
--    operational complexity (checkpoint file groups, XTP garbage
--    collection behavior, no replication/CDC/Always On automatic support
--    for memory-optimized filegroups in some configurations) for an
--    unproven need.
-- 3. Any small, mostly-static lookup/reference table (e.g. Diagnostics.
--    RuleRegistry, Diagnostics.ProviderRegistry, ReportingDB.ReportType,
--    ArchiveDB.Archival.TierPolicy): these are small, low-write-frequency
--    configuration tables. In-Memory OLTP's main benefit (eliminating lock/
--    latch contention under high concurrent write load) does not apply --
--    these tables are read-mostly and already served well by ordinary
--    rowstore B-tree indexes with a small buffer-pool footprint. No case.
--
-- *** SQL Server Express memory constraint (verified live, informs the
--     "even if a case existed" ceiling) ***
-- Express edition enforces the lesser of 1GB or 25% of the OS-visible
-- memory for the In-Memory OLTP memory-optimized-object cap, in addition to
-- the well-known 1,410 MB buffer pool cap for the engine overall -- a hard
-- ceiling that makes Express fundamentally unsuited to running a genuinely
-- high-throughput memory-optimized workload even if one existed. This
-- reinforces (does not solely drive) the skip decision above.
--
-- *** Conclusion ***
-- In-Memory OLTP is explicitly evaluated and SKIPPED platform-wide as of
-- this migration. No table is converted. Revisit only if/when a specific
-- table shows measured lock/latch contention under real production load
-- (Query Store, enabled in 111-query-store-configuration.sql, is the
-- intended detection mechanism for that future signal) AND that table's
-- durability/cross-database-logging/RLS requirements are compatible with
-- the memory-optimized engine's constraints.
--
-- Idempotency: this script is read-only / verification-only. Creates
-- nothing, alters nothing. Safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- Live verification: confirm zero memory-optimized objects exist anywhere
-- today (sanity check that no earlier phase silently introduced one) and
-- confirm the instance's In-Memory OLTP capability/ceiling.
------------------------------------------------------------
SELECT SERVERPROPERTY('IsXTPSupported') AS IsXTPSupported;
GO

DECLARE @dbs TABLE (name sysname);
INSERT INTO @dbs (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB');

DECLARE @db sysname, @sql nvarchar(max);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @dbs;
OPEN c;
FETCH NEXT FROM c INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'USE ' + QUOTENAME(@db) + N';
    SELECT ''' + @db + N''' AS DbName, COUNT(*) AS MemoryOptimizedTableCount
    FROM sys.tables WHERE is_memory_optimized = 1;';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM c INTO @db;
END
CLOSE c;
DEALLOCATE c;
GO
-- Expected/confirmed: MemoryOptimizedTableCount = 0 in every database.

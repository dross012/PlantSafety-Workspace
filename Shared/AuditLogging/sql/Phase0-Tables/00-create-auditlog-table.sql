-- =============================================
-- Shared.AuditLogging
-- Script:        00-create-auditlog-table.sql
-- Purpose:       Canonical, idempotent CREATE TABLE script for
--                [PlantSafety.AuditDB].dbo.AuditLog -- the single, shared,
--                cross-database, append-only platform audit-trail table
--                written to by hundreds of stored procedures across all 8
--                module databases (PlantSafety.CoreDB, ArchiveDB,
--                CloudIdentityDB, PredictiveDB, ReportingDB, IntegrationDB,
--                DiagnosticsDB, ExportImportDB) via a per-database
--                dbo.AuditLog synonym.
--
-- EXTRACTION NOTE (2026-07-26): this table previously had NO tracked
-- CREATE TABLE script anywhere in this solution. A repo-wide search of
-- PlantSafety/**/*.sql (including every Persistence\Migrations\Phase*
-- script) found only:
--   - PlantSafety.Infrastructure\Persistence\Migrations\Phase8-Structural\
--     83-synonyms-audit-error.sql -- creates a per-database dbo.AuditLog
--     synonym pointing at [PlantSafety.AuditDB].dbo.AuditLog, but does NOT
--     create the underlying table (its own header comment describes the
--     table as pre-existing infrastructure, "verified live to work
--     correctly", not something this migration series stands up).
--   - PlantSafety.Infrastructure\Persistence\Migrations\Phase14-Governance\
--     141-extended-properties-phase8-structural.sql -- documents the
--     synonym object, not the table.
--   - Roughly 400 usp_*_Insert/Update/View stored procedures across every
--     Phase6-StoredProcedures\*-procedures.sql file (e.g. 60-coredb-
--     procedures.sql) plus Phase17-Reliability\170-fix-errorlog-masking-
--     bug.sql -- these INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog but
--     never create it.
-- Conclusion: dbo.AuditLog is pre-existing, unversioned platform
-- infrastructure (created directly on SQLEXPRESS outside of any tracked
-- migration script) -- the same pattern this solution already documents for
-- other out-of-band databases (see PlantSafety\docs\database\
-- placeholder-databases.md for the sibling "exists live, no in-repo
-- creation script" pattern). There is therefore no PlantSafety-local
-- "CREATE TABLE AuditLog" script to relocate; what is being extracted here
-- is the table's *definition*, reverse-engineered from every confirmed
-- live call site, so it is finally version-controlled in the module that
-- now owns the read (and, going forward, definitional) contract for it.
--
-- COLUMN LIST -- identical across all ~400 confirmed INSERT call sites
-- (two shapes: success-path 5/9-column inserts and catch-block 11-column
-- inserts, always the same names/order); cross-checked against
-- Shared.AuditLogging.Abstractions\Models\AuditLogEntry.cs, which documents
-- the same evidence trail:
--   TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
--   ViewerAccessed, ErrorContext, Severity, Module, BeforeSnapshot, AfterSnapshot
--
-- DATA TYPES -- inferred from confirmed, internally-consistent call sites:
--   - TableName, Module        nvarchar(128) -- matches usp_SearchAuditLog's
--                               @TableName/@Module nvarchar(128) parameters.
--   - SourceDatabase            nvarchar(128) -- matches every catch block's
--                               @__SourceDatabase nvarchar(128) = DB_NAME().
--   - Operation                 nvarchar(50)  -- matches usp_SearchAuditLog's
--                               @Operation nvarchar(50).
--   - ChangedBy                 nvarchar(128) -- matches @ChangedBy
--                               nvarchar(128) (holds SYSTEM_USER).
--   - ChangeTime                 datetime2     -- matches @FromUtc/@ToUtc
--                               datetime2 range filters and SYSUTCDATETIME()
--                               inserts.
--   - ViewerAccessed             bit           -- only ever inserted as 0/1.
--   - ErrorContext               nvarchar(400) -- matches the @__ErrorContext
--                               nvarchar(400) local variable declared in
--                               every stored procedure's catch block, and
--                               usp_SearchAuditLog's @SearchText nvarchar(400)
--                               LIKE filter against this column.
--   - Severity                   int           -- matches usp_SearchAuditLog's
--                               @Severity int parameter and ERROR_SEVERITY()
--                               (int) inserts.
--   - BeforeSnapshot/AfterSnapshot  nvarchar(max) -- ASSUMPTION, flagged: no
--                               confirmed INSERT anywhere in this codebase
--                               ever passes a non-NULL value for these two
--                               columns, so no width/type evidence exists.
--                               nvarchar(max) is the least-risky generic
--                               choice for a reserved, currently-always-NULL
--                               "structured snapshot" column. VERIFY AGAINST
--                               THE LIVE INFORMATION_SCHEMA BEFORE RELYING ON
--                               THIS TYPE.
--
-- NO PRIMARY KEY -- deliberate, not an oversight. No identity/primary-key
-- column name is evidenced anywhere in the ~400 confirmed INSERT statements
-- scanned (every one supplies an explicit column list, none of which
-- include a StrongId/Id-suffixed column), and this module's own
-- docs\README.md and AuditLogEntry.cs already record this as a confirmed,
-- deliberate finding: "No invented primary key. No identity/primary-key
-- column is evidenced in the scanned migration scripts for dbo.AuditLog, so
-- none is invented here." This script follows that same finding rather than
-- inventing a StrongId surrogate key on a live, multi-writer table without
-- verifying against the real deployed schema first.
--
-- NO CANONICAL METADATA COLUMNS (CreatedUtc/CreatedBy/UpdatedUtc/UpdatedBy/
-- IsDeleted) -- also deliberate. ChangedBy/ChangeTime already fill the
-- equivalent role for each row, and the table is documented and verified
-- append-only (docs\README.md's "Safety invariants": nothing may INSERT,
-- UPDATE, or DELETE against it outside the existing write path). There is
-- no meaningful "UpdatedBy" for a row that must never be updated, and no
-- "IsDeleted" for a table with no supported delete path. Adding these
-- columns would not match any confirmed INSERT statement in the codebase
-- and would require touching all ~400 existing writer call sites, which is
-- out of scope for a table-relocation-only extraction.
--
-- POLICY DEVIATION FLAGGED FOR HUMAN REVIEW: the general "every table gets
-- a StrongId clustered PK + 5 canonical metadata columns" mandate is
-- deliberately NOT applied here, for the two reasons above.
-- `sql-database-skill\references\00-canonical-templates.md` and
-- `00-index.md` -- cited as this table's authoritative source -- were
-- searched for repo-wide and do not exist anywhere in this repository as of
-- this script's authoring. In their absence, this script grounds itself in
-- the only available ground truth: live, confirmed call-site evidence plus
-- this module's own prior, already-reviewed judgment calls (docs\README.md,
-- AuditLogEntry.cs). A human should re-run schema validation against the
-- real canonical-templates.md, once located/provided, to confirm this table
-- remains an intentional, documented exception rather than a gap.
--
-- IDEMPOTENT / NON-DESTRUCTIVE: guarded by an OBJECT_ID existence check.
-- This table is believed to already exist live on the target AuditDB (per
-- the evidence above), so this script is expected to no-op there; it exists
-- so the definition is finally version-controlled and reviewable, and so a
-- genuinely fresh environment can stand up an empty AuditLog table with the
-- confirmed shape. Does not ALTER or DROP any existing table/column -- per
-- deletion-discipline policy, no removal happens in this script.
-- =============================================

USE [PlantSafety.AuditDB]
GO

IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        TableName       nvarchar(128)   NOT NULL,
        SourceDatabase  nvarchar(128)   NOT NULL,
        Operation       nvarchar(50)    NOT NULL,
        ChangedBy       nvarchar(128)   NOT NULL,
        ChangeTime      datetime2       NOT NULL,
        ViewerAccessed  bit             NOT NULL CONSTRAINT DF_AuditLog_ViewerAccessed DEFAULT (0),
        ErrorContext    nvarchar(400)   NULL,
        Severity        int             NOT NULL CONSTRAINT DF_AuditLog_Severity DEFAULT (0),
        Module          nvarchar(128)   NOT NULL,
        BeforeSnapshot  nvarchar(max)   NULL,
        AfterSnapshot   nvarchar(max)   NULL
    );
END
GO

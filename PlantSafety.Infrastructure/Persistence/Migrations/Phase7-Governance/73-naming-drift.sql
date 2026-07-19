-- =============================================
-- Migration:     73-naming-drift.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Naming-convention drift remediation (Governance
--                remediation item #4).
--
-- Object 1: PlantSafety.CoreDB.Operational.AlertSummary index
--   'UX_AlertSummary_Key' -> renamed to 'UQ_Operational_AlertSummary_Key'.
--   Verified live: this is a NONCLUSTERED UNIQUE INDEX (not a constraint),
--   on columns (SummaryDateKey, TenantId, AlertType, Severity, Status).
--   The live-verified convention across the platform for composite unique
--   indexes is `UQ_<Schema>_<Entity>_<Suffix>` (confirmed by cross-checking
--   every other unique index in all 8 in-scope databases, e.g.
--   UQ_Diagnostics_DiagnosticDailySummary_Key, UQ_System_SchemaMigrations_
--   ScriptName, UQ_Integration_ContractSchema_NameVersion) -- so the target
--   name is UQ_Operational_AlertSummary_Key (the "_Key" suffix mirrors the
--   existing composite-key convention seen on DiagnosticDailySummary).
--   Table Operational.AlertSummary is empty (0 rows, live-verified) and the
--   index has no documented external callers, so a plain sp_rename is safe
--   -- no drop/recreate needed, no data risk.
--
-- Objects 2/3: usp_WriteAuditLog / usp_WriteErrorLog / TR_ErrorLog_NoDelete
--   -- NOT renamed. See canonical-templates.md update (companion change,
--   outside this migrations folder) documenting this as a formal, approved
--   naming-convention exception for shared cross-database platform-utility
--   objects. Rationale documented there; summary: (a) live-verified ZERO
--   callers anywhere -- scanned sys.sql_modules text across all 8 in-scope
--   databases plus a full source-tree grep of C:\Users\dross\source for
--   the literal names, finding no references at all, so renaming would be
--   mechanically safe today; BUT (b) renaming the two procedures to the
--   platform's `sp_<Entity>_<Operation>` convention would trade a cosmetic
--   fix for a real, well-documented SQL Server anti-pattern: any user
--   procedure named with an `sp_` prefix is first looked up in the `master`
--   database by the engine on every call, adding resolution overhead and
--   creating a latent risk of being shadowed by a future same-named system
--   procedure. Since these two procedures are explicitly designed
--   ("Intended for cross-database callers") as a stable, frequently-called,
--   cross-database platform API surface, avoiding the sp_ prefix here is
--   the correct engineering choice, not an oversight. The `usp_` prefix
--   (and `TR_` for the companion trigger) is retained and formally
--   documented as an intentional exception lane rather than mass-renamed.
-- =============================================
SET NOCOUNT ON;
GO

USE [PlantSafety.CoreDB];
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    JOIN sys.schemas s ON o.schema_id = s.schema_id
    WHERE s.name = 'Operational' AND o.name = 'AlertSummary' AND i.name = 'UX_AlertSummary_Key'
)
BEGIN
    EXEC sp_rename N'Operational.AlertSummary.UX_AlertSummary_Key', N'UQ_Operational_AlertSummary_Key', N'INDEX';
    PRINT 'Renamed Operational.AlertSummary.UX_AlertSummary_Key -> UQ_Operational_AlertSummary_Key.';
END
ELSE IF EXISTS (
    SELECT 1
    FROM sys.indexes i
    JOIN sys.objects o ON i.object_id = o.object_id
    JOIN sys.schemas s ON o.schema_id = s.schema_id
    WHERE s.name = 'Operational' AND o.name = 'AlertSummary' AND i.name = 'UQ_Operational_AlertSummary_Key'
)
    PRINT 'Operational.AlertSummary already carries UQ_Operational_AlertSummary_Key -- skipped (idempotent re-run).';
ELSE
    PRINT 'WARNING: neither UX_AlertSummary_Key nor UQ_Operational_AlertSummary_Key found on Operational.AlertSummary -- verify manually.';
GO

------------------------------------------------------------
-- Verification
------------------------------------------------------------
SELECT s.name AS schema_name, o.name AS table_name, i.name AS index_name, i.is_unique, i.type_desc
FROM sys.indexes i
JOIN sys.objects o ON i.object_id = o.object_id
JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE o.name = 'AlertSummary';
GO

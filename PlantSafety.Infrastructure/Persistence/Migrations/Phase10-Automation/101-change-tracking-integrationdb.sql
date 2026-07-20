-- =============================================
-- Phase 10 Automation & Integration
-- Script:        101-change-tracking-integrationdb.sql
-- Author:        Automation & Integration Agent
-- Create date:   2026-07-19
-- Purpose:       Governance item #2 (CDC / Change Tracking).
--
-- *** CDC CAPABILITY CHECK (live-verified 2026-07-19, do not assume) ***
-- SELECT SERVERPROPERTY('Edition') -> 'Express Edition (64-bit)'.
-- EXEC sys.sp_cdc_enable_db against PlantSafety.IntegrationDB returned:
--   Msg 22988: "This instance of SQL Server is the Express Edition (64-bit).
--   Change data capture is only available in the Enterprise, Enterprise
--   Developer, Enterprise Evaluation, Standard, and Standard Developer
--   editions."
-- So CDC is genuinely NOT available on this instance -- confirmed by a live
-- failed call, not assumed from memory. CDC is therefore fully out of scope
-- until the instance is upgraded to Standard+ edition (prep note only, no
-- artifact to build today).
--
-- Change Tracking (the lightweight, Express-supported alternative) IS
-- available and is what this script enables.
--
-- *** WHICH TABLES, AND WHY (genuinely justified, not "everywhere") ***
-- Per the CrossDomain Architecture & Design Manual: "All crossdomain data
-- flows through the Integration Hub" and the Hub's certification domains
-- explicitly include "Schema Drift & Change Detection" and "Sync
-- Architecture" / "Delta Sync" as first-class, named concerns for external
-- system adapters consuming PlantSafety's canonical contract/routing
-- registry. That is a genuine, source-grounded justification for Change
-- Tracking specifically on the contract-registry and routing-config tables
-- in PlantSafety.IntegrationDB -- external adapters need to detect and pull
-- only what changed since their last sync, without CDC's heavier full
-- history capture (which isn't available on Express anyway) and without a
-- full table rescan every poll:
--   - Integration.ContractSchema  (canonical schema/message-type registry)
--   - Integration.ContractVersion (contract lifecycle/version registry)
--   - Integration.ContractField   (per-field contract definitions)
--   - Integration.RoutingTableEntry (subscriber/routing config external
--     adapters and the routing engine itself need incremental awareness of)
--
-- *** WHAT WAS DELIBERATELY SKIPPED, AND WHY (No-Guessing Rule) ***
-- - Integration.IntegrationLogEntry / ReplayRecord / ModuleHealthSnapshot /
--   *DailySummary tables: these are operational logs/telemetry, not
--   reference/config data an external system incrementally syncs against;
--   they are already governed by the Phase 7 retention-purge jobs and (for
--   ReplayRecord) the new Phase 10 Service Broker replay queue. Enabling
--   Change Tracking on high-churn append-only log tables would add pure
--   overhead with no real consumer, so this is a deliberate skip, not an
--   oversight.
-- - PlantSafety.DiagnosticsDB: no grounding was found in the Diagnostics
--   Rules & Design Manual, the Diagnostics SQL Server Script Manual, or the
--   Diagnostics Integration Tests Manual for an EXTERNAL system consuming
--   DiagnosticsDB tables via incremental delta sync (the task prompt's
--   mention of DiagnosticsDB was offered as an example to evaluate, not a
--   confirmed requirement). Per the No-Guessing Rule for schema generation,
--   this is flagged and skipped rather than invented. Revisit if/when a
--   concrete external consumer of DiagnosticsDB data is documented.
--
-- Idempotency: ALTER DATABASE ... SET CHANGE_TRACKING ON is safe to rerun
-- (SQL Server no-ops if already enabled with matching retention settings);
-- each ALTER TABLE ... ENABLE CHANGE_TRACKING is existence-guarded via
-- sys.change_tracking_tables. No destructive action; additive only.
-- =============================================
SET NOCOUNT ON;
GO

USE [PlantSafety.IntegrationDB];
GO

------------------------------------------------------------
-- 1. Database-level Change Tracking.
--    Retention: 14 days is a placeholder (no confirmed SLA from the
--    CrossDomain manual for how long an external adapter may lag before a
--    full resync is required) -- AUTO_CLEANUP ON is the default/recommended
--    setting. Flagging 14 days as a placeholder for the user to confirm
--    against real external-adapter polling-frequency SLAs.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_databases WHERE database_id = DB_ID())
BEGIN
    ALTER DATABASE [PlantSafety.IntegrationDB]
        SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 14 DAYS, AUTO_CLEANUP = ON);
END
GO

------------------------------------------------------------
-- 2. Table-level Change Tracking on the 4 justified contract/routing
--    registry tables. TRACK_COLUMNS_UPDATED = ON so adapters can see which
--    columns changed (not just that a row changed), useful for narrow
--    schema-drift detection without pulling full rows every time.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Integration.ContractSchema'))
    ALTER TABLE Integration.ContractSchema ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);
GO
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Integration.ContractVersion'))
    ALTER TABLE Integration.ContractVersion ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);
GO
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Integration.ContractField'))
    ALTER TABLE Integration.ContractField ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);
GO
IF NOT EXISTS (SELECT 1 FROM sys.change_tracking_tables WHERE object_id = OBJECT_ID('Integration.RoutingTableEntry'))
    ALTER TABLE Integration.RoutingTableEntry ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);
GO

------------------------------------------------------------
-- 3. Verification.
------------------------------------------------------------
SELECT d.name AS DatabaseName, ctd.is_auto_cleanup_on, ctd.retention_period, ctd.retention_period_units_desc
FROM sys.change_tracking_databases ctd
JOIN sys.databases d ON d.database_id = ctd.database_id
WHERE d.name = 'PlantSafety.IntegrationDB';
GO

SELECT OBJECT_SCHEMA_NAME(object_id) AS SchemaName, OBJECT_NAME(object_id) AS TableName, is_track_columns_updated_on
FROM sys.change_tracking_tables
ORDER BY 1, 2;
GO

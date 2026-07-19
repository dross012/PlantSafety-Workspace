-- =============================================
-- Phase 3 Architectural Remediation
-- Script:        32-workerwellbeing-index-fix.sql
-- Purpose:       Fix the TenantId-leading index gap on
--                WorkerWellBeing.WorkerWellBeing flagged in the Phase 2
--                validation pass: the table's only index touching TenantId
--                has it as an INCLUDE column, not a leading key column, so
--                there is no efficient tenant-scoped seek.
--
-- Verified live 2026-07-18 against (local)\SQLEXPRESS, PlantSafety.CoreDB:
--   - Table: [WorkerWellBeing].[WorkerWellBeing] (schema name = table name,
--     same pattern as AiGovernance, DigitalTwin, EnterpriseFabric,
--     HumanFactors, OperationalSafety, Sustainability, etc. in this DB).
--   - Existing indexes:
--       PK_WorkerWellBeing_WorkerWellBeing              CLUSTERED (WorkerWellBeingId)
--       IX_WorkerWellBeing_WorkerWellBeing_WorkerId_RecordedAtUtc  NONCLUSTERED (WorkerId, RecordedAtUtc)
--       IX_WorkerWellBeing_WorkerWellBeing_WellBeingRiskLevel      NONCLUSTERED (WellBeingRiskLevel) INCLUDE (TenantId)
--       IX_WorkerWellBeing_WorkerWellBeing_Status                  NONCLUSTERED (Status)
--     Confirms the gap: TenantId only ever appears as an INCLUDE column
--     (on the WellBeingRiskLevel index), never as a leading key column, so
--     no index supports an efficient tenant-scoped seek.
--   - Table confirmed 0 rows.
--
-- Fix approach: ADD a new index rather than modify
-- IX_WorkerWellBeing_WorkerWellBeing_WellBeingRiskLevel, per the task's
-- guidance to prefer adding when modifying risks losing existing coverage
-- (that index serves a different, already-useful access pattern --
-- risk-level lookups with a tenant-scoped covering payload -- and should be
-- left intact).
--
-- Naming: follows the live IX_<Schema>_<Table>_<Columns> convention used
-- throughout PlantSafety.CoreDB for schema-equals-table-name modules (e.g.
-- IX_AiGovernance_AiGovernance_TenantId, IX_DigitalTwin_DigitalTwin_TenantId,
-- IX_HumanFactors_HumanFactors_TenantId), all of which are single-column
-- leading-TenantId indexes with no INCLUDE columns -- the new index below
-- matches that established, dominant pattern exactly rather than inventing
-- additional included columns.
--
-- Idempotent: guarded by sys.indexes existence check; safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_WorkerWellBeing_WorkerWellBeing_TenantId'
      AND object_id = OBJECT_ID(N'[WorkerWellBeing].[WorkerWellBeing]')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkerWellBeing_WorkerWellBeing_TenantId
        ON [WorkerWellBeing].[WorkerWellBeing] (TenantId);
END;
GO

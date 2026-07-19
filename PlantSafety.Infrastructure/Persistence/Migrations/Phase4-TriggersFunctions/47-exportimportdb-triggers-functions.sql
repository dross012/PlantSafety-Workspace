USE [PlantSafety.ExportImportDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        47-exportimportdb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   ExportImport.ExportBatch, ExportOperation, ExportRecord, ImportBatch,
--   ImportOperation, and ImportRecord carry ConcurrencyToken +
--   UpdatedUtc/UpdatedBy; AFTER UPDATE triggers force
--   UpdatedUtc = SYSUTCDATETIME() on every update -- see the in-body comment
--   for why UpdatedBy is left to the app/SP layer. ExportOperationError and
--   ImportOperationError carry neither ConcurrencyToken nor
--   UpdatedUtc/UpdatedBy (append-only error records, per the
--   exportimport_rules_design.md 5-error-category model, never updated
--   post-insert) and are intentionally excluded.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--
-- SECTION 3 -- Functions: NONE CREATED
--   No TenantId column exists on any ExportImportDB table, so
--   fn_IsWithinTenant is not applicable. No computed business formula
--   analogous to RiskManagement.RiskScore was found in the ExportImport
--   domain source. No scalar/inline-TVF functions are warranted for this
--   database.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [ExportImport].[trg_ExportBatch_UpdateAudit]
ON [ExportImport].[ExportBatch]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ExportBatch] AS t
    INNER JOIN inserted AS i
        ON t.[ExportBatchId] = i.[ExportBatchId];
END
GO

CREATE OR ALTER TRIGGER [ExportImport].[trg_ExportOperation_UpdateAudit]
ON [ExportImport].[ExportOperation]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ExportOperation] AS t
    INNER JOIN inserted AS i
        ON t.[ExportOperationId] = i.[ExportOperationId];
END
GO

CREATE OR ALTER TRIGGER [ExportImport].[trg_ExportRecord_UpdateAudit]
ON [ExportImport].[ExportRecord]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ExportRecord] AS t
    INNER JOIN inserted AS i
        ON t.[ExportRecordId] = i.[ExportRecordId];
END
GO

CREATE OR ALTER TRIGGER [ExportImport].[trg_ImportBatch_UpdateAudit]
ON [ExportImport].[ImportBatch]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ImportBatch] AS t
    INNER JOIN inserted AS i
        ON t.[ImportBatchId] = i.[ImportBatchId];
END
GO

CREATE OR ALTER TRIGGER [ExportImport].[trg_ImportOperation_UpdateAudit]
ON [ExportImport].[ImportOperation]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ImportOperation] AS t
    INNER JOIN inserted AS i
        ON t.[ImportOperationId] = i.[ImportOperationId];
END
GO

CREATE OR ALTER TRIGGER [ExportImport].[trg_ImportRecord_UpdateAudit]
ON [ExportImport].[ImportRecord]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ExportImport].[ImportRecord] AS t
    INNER JOIN inserted AS i
        ON t.[ImportRecordId] = i.[ImportRecordId];
END
GO



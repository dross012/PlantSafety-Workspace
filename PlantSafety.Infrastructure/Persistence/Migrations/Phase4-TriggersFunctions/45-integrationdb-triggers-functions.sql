USE [PlantSafety.IntegrationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        45-integrationdb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   Integration.ContractField, Integration.ContractSchema,
--   Integration.ContractVersion, and Integration.RoutingTableEntry carry
--   ConcurrencyToken + UpdatedUtc/UpdatedBy; AFTER UPDATE triggers force
--   UpdatedUtc = SYSUTCDATETIME() on every update -- see the in-body comment
--   for why UpdatedBy is left to the app/SP layer. IntegrationLogEntry,
--   ModuleHealthSnapshot, and ReplayRecord carry neither ConcurrencyToken nor
--   UpdatedUtc/UpdatedBy (append-only event/log/replay records, never
--   updated post-insert) and are intentionally excluded.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--
-- SECTION 3 -- Functions: NONE CREATED
--   No TenantId column exists on any IntegrationDB table, so
--   fn_IsWithinTenant is not applicable. No computed business formula
--   analogous to RiskManagement.RiskScore was found in the integration
--   domain source. No scalar/inline-TVF functions are warranted for this
--   database.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [Integration].[trg_ContractField_UpdateAudit]
ON [Integration].[ContractField]
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
    FROM [Integration].[ContractField] AS t
    INNER JOIN inserted AS i
        ON t.[ContractFieldId] = i.[ContractFieldId];
END
GO

CREATE OR ALTER TRIGGER [Integration].[trg_ContractSchema_UpdateAudit]
ON [Integration].[ContractSchema]
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
    FROM [Integration].[ContractSchema] AS t
    INNER JOIN inserted AS i
        ON t.[ContractSchemaId] = i.[ContractSchemaId];
END
GO

CREATE OR ALTER TRIGGER [Integration].[trg_ContractVersion_UpdateAudit]
ON [Integration].[ContractVersion]
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
    FROM [Integration].[ContractVersion] AS t
    INNER JOIN inserted AS i
        ON t.[ContractVersionId] = i.[ContractVersionId];
END
GO

CREATE OR ALTER TRIGGER [Integration].[trg_RoutingTableEntry_UpdateAudit]
ON [Integration].[RoutingTableEntry]
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
    FROM [Integration].[RoutingTableEntry] AS t
    INNER JOIN inserted AS i
        ON t.[RoutingTableEntryId] = i.[RoutingTableEntryId];
END
GO



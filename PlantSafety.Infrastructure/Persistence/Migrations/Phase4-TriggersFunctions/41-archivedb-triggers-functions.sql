USE [PlantSafety.ArchiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        41-archivedb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   Both mutable ArchiveDB tables (Archival.ArchivalJob, Archival.TierPolicy)
--   carry ConcurrencyToken + UpdatedUtc/UpdatedBy. AFTER UPDATE triggers force
--   UpdatedUtc = SYSUTCDATETIME() on every update -- see the in-body comment
--   for why UpdatedBy is intentionally left to the app/SP layer.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--
-- SECTION 3 -- Functions: NONE CREATED
--   ArchiveDB has no TenantId-scoped multi-tenancy (only ArchivalJob carries
--   TenantId; TierPolicy does not, so a shared fn_IsWithinTenant helper would
--   not be broadly reusable here) and no computed business formula analogous
--   to RiskManagement.RiskScore exists in the Archival domain source. No
--   scalar/inline-TVF functions are warranted for this database.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [Archival].[trg_ArchivalJob_UpdateAudit]
ON [Archival].[ArchivalJob]
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
    FROM [Archival].[ArchivalJob] AS t
    INNER JOIN inserted AS i
        ON t.[ArchivalJobId] = i.[ArchivalJobId];
END
GO

CREATE OR ALTER TRIGGER [Archival].[trg_TierPolicy_UpdateAudit]
ON [Archival].[TierPolicy]
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
    FROM [Archival].[TierPolicy] AS t
    INNER JOIN inserted AS i
        ON t.[TierPolicyId] = i.[TierPolicyId];
END
GO



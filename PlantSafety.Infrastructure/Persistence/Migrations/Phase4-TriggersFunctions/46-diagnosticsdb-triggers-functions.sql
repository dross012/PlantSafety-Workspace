USE [PlantSafety.DiagnosticsDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        46-diagnosticsdb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   8 DiagnosticsDB tables carry ConcurrencyToken + UpdatedUtc/UpdatedBy:
--   DiagnosticAttachment, DiagnosticEventDimension, DiagnosticSnapshot,
--   EngineRun, ProviderRegistry, RuleRegistry, TenantEngineRun,
--   TenantRuleOverride. AFTER UPDATE triggers force
--   UpdatedUtc = SYSUTCDATETIME() on every update -- see the in-body comment
--   for why UpdatedBy is left to the app/SP layer. DiagnosticDailySummary,
--   DiagnosticEvent, DiagnosticEventArchive, DiagnosticEventFact,
--   DiagnosticMetadata, and TenantDiagnosticEvent carry neither
--   ConcurrencyToken nor UpdatedUtc/UpdatedBy (event/fact/archive/summary
--   records are append-only or materialized-summary by design per
--   diagnostics_sql_manual.md) and are intentionally excluded.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--   (diagnostics_sql_manual.md separately notes existing DML triggers on
--   DiagnosticEvent/DiagnosticSnapshot are defense-in-depth AFTER triggers
--   catching direct-T-SQL bypass, not soft-delete converters -- consistent
--   with this rule; none currently exist live in this database, and none of
--   that shape are added here since it would duplicate the SP-layer's own
--   validation without new coverage.)
--
-- SECTION 3 -- Functions
--   dbo.fn_IsWithinTenant                    -- scalar, deterministic
--                                                tenant-scoping helper (11
--                                                DiagnosticsDB tables carry
--                                                TenantId).
--   Diagnostics.fn_TenantEngineRun_Active     -- inline TVF, tenant-scoped
--   Diagnostics.fn_TenantRuleOverride_Active  -- inline TVF, tenant-scoped
--   Diagnostics.fn_TenantDiagnosticEvent_Active -- inline TVF, tenant-scoped
--   These three "Tenant*" overlay tables are the ones genuinely designed
--   around per-tenant filtering (by naming and shape), so they get the
--   fn_<Entity>_Active pattern; the remaining 8 non-tenant-prefixed
--   Diagnostics tables (ProviderRegistry, RuleRegistry, EngineRun, etc.) are
--   engine-wide/global registries, not tenant-partitioned data, so a
--   tenant-scoped Active TVF would not reflect their real usage and is
--   intentionally omitted.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [Diagnostics].[trg_DiagnosticAttachment_UpdateAudit]
ON [Diagnostics].[DiagnosticAttachment]
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
    FROM [Diagnostics].[DiagnosticAttachment] AS t
    INNER JOIN inserted AS i
        ON t.[DiagnosticAttachmentId] = i.[DiagnosticAttachmentId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_DiagnosticEventDimension_UpdateAudit]
ON [Diagnostics].[DiagnosticEventDimension]
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
    FROM [Diagnostics].[DiagnosticEventDimension] AS t
    INNER JOIN inserted AS i
        ON t.[DiagnosticEventDimensionId] = i.[DiagnosticEventDimensionId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_DiagnosticSnapshot_UpdateAudit]
ON [Diagnostics].[DiagnosticSnapshot]
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
    FROM [Diagnostics].[DiagnosticSnapshot] AS t
    INNER JOIN inserted AS i
        ON t.[DiagnosticSnapshotId] = i.[DiagnosticSnapshotId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_EngineRun_UpdateAudit]
ON [Diagnostics].[EngineRun]
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
    FROM [Diagnostics].[EngineRun] AS t
    INNER JOIN inserted AS i
        ON t.[EngineRunId] = i.[EngineRunId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_ProviderRegistry_UpdateAudit]
ON [Diagnostics].[ProviderRegistry]
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
    FROM [Diagnostics].[ProviderRegistry] AS t
    INNER JOIN inserted AS i
        ON t.[ProviderRegistryId] = i.[ProviderRegistryId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_RuleRegistry_UpdateAudit]
ON [Diagnostics].[RuleRegistry]
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
    FROM [Diagnostics].[RuleRegistry] AS t
    INNER JOIN inserted AS i
        ON t.[RuleRegistryId] = i.[RuleRegistryId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_TenantEngineRun_UpdateAudit]
ON [Diagnostics].[TenantEngineRun]
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
    FROM [Diagnostics].[TenantEngineRun] AS t
    INNER JOIN inserted AS i
        ON t.[TenantEngineRunId] = i.[TenantEngineRunId];
END
GO

CREATE OR ALTER TRIGGER [Diagnostics].[trg_TenantRuleOverride_UpdateAudit]
ON [Diagnostics].[TenantRuleOverride]
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
    FROM [Diagnostics].[TenantRuleOverride] AS t
    INNER JOIN inserted AS i
        ON t.[TenantRuleOverrideId] = i.[TenantRuleOverrideId];
END
GO


-- ===================== SECTION 3: Functions =====================

-- =============================================
-- Function:      dbo.fn_IsWithinTenant
-- Purpose:       Deterministic tenant-scoping predicate helper for the 11
--                DiagnosticsDB tables that carry TenantId (the Tenant*
--                overlay tables plus DiagnosticAttachment/DiagnosticSnapshot/
--                etc.). Reusable in SP/view WHERE clauses instead of
--                duplicating the NULL-safe comparison everywhere.
-- Determinism:   Pure comparison, no table refs, no nondeterministic calls.
-- =============================================
CREATE OR ALTER FUNCTION dbo.fn_IsWithinTenant
(
    @TenantId    uniqueidentifier,
    @RowTenantId uniqueidentifier
)
RETURNS bit
WITH SCHEMABINDING
AS
BEGIN
    RETURN CASE WHEN @TenantId IS NOT NULL AND @RowTenantId IS NOT NULL
                  AND @TenantId = @RowTenantId
                THEN CAST(1 AS bit)
                ELSE CAST(0 AS bit)
           END;
END
GO

-- =============================================
-- Function:      Diagnostics.fn_TenantEngineRun_Active
-- Purpose:       Inline TVF: non-deleted TenantEngineRun records for a
--                tenant. Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION Diagnostics.fn_TenantEngineRun_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.TenantEngineRunId, e.TenantId, e.StartedAtUtc, e.CompletedAtUtc,
        e.IsSuccess, e.TotalRulesEvaluated, e.PassCount, e.FailCount,
        e.SkipCount, e.ErrorCount, e.EngineVersion,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM Diagnostics.TenantEngineRun AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      Diagnostics.fn_TenantRuleOverride_Active
-- Purpose:       Inline TVF: non-deleted TenantRuleOverride records for a
--                tenant. Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION Diagnostics.fn_TenantRuleOverride_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.TenantRuleOverrideId, e.TenantId, e.RuleRegistryId,
        e.OverrideSeverity, e.IsSuppressed, e.Notes,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM Diagnostics.TenantRuleOverride AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      Diagnostics.fn_TenantDiagnosticEvent_Active
-- Purpose:       Inline TVF: non-deleted TenantDiagnosticEvent records for a
--                tenant. TenantDiagnosticEvent is append-only (no
--                ConcurrencyToken/UpdatedUtc/UpdatedBy -- see Section 1
--                header note), so this TVF omits those columns and reflects
--                the table's real shape. Explicit column list, no business
--                logic.
-- =============================================
CREATE OR ALTER FUNCTION Diagnostics.fn_TenantDiagnosticEvent_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.TenantDiagnosticEventId, e.TenantId, e.CorrelationId, e.Severity,
        e.Category, e.Module, e.Operation, e.OperationStatus, e.Message,
        e.Detail, e.MetadataJson, e.OccurredAtUtc,
        e.CreatedUtc, e.CreatedBy, e.IsDeleted
    FROM Diagnostics.TenantDiagnosticEvent AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

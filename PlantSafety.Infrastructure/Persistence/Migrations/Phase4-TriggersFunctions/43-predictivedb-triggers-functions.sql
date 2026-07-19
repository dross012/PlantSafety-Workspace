USE [PlantSafety.PredictiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        43-predictivedb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
--
-- SECTION 1 -- Update-audit triggers
--   PredictiveSafety.SafetyPrediction is the only mutable PredictiveDB table
--   carrying ConcurrencyToken + UpdatedUtc/UpdatedBy (composite PK:
--   CreatedUtc + SafetyPredictionId, matching its time-partitioned layout).
--   PredictionEvidence has no ConcurrencyToken/UpdatedUtc/UpdatedBy pair
--   (append-only evidence records tied 1:1 to a prediction) and is
--   intentionally excluded. The AFTER UPDATE trigger forces
--   UpdatedUtc = SYSUTCDATETIME() on every update of SafetyPrediction -- see
--   the in-body comment for why UpdatedBy is left to the app/SP layer.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   Per enterprise_data_database_architecture_master.md / sql-database-skill:
--   triggers must be "AFTER only (never INSTEAD OF)" and restricted to
--   "audit/error/summary-refresh orchestration only, never business logic".
--   Converting DELETE to soft-delete needs an INSTEAD OF trigger, which is
--   explicitly forbidden; this stays an application/SP-layer responsibility.
--
-- SECTION 3 -- Functions: NONE CREATED
--   No TenantId column exists on either PredictiveDB table, so
--   fn_IsWithinTenant is not applicable. No computed business formula
--   analogous to RiskManagement.RiskScore was found in the predictive-safety
--   domain source (confidence/likelihood scoring for SafetyPrediction is
--   produced by the ML pipeline upstream, not a deterministic SQL-expressible
--   formula, so replicating it in T-SQL would be inventing logic, not
--   grounding it). No scalar/inline-TVF functions are warranted for this
--   database.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [PredictiveSafety].[trg_SafetyPrediction_UpdateAudit]
ON [PredictiveSafety].[SafetyPrediction]
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
    FROM [PredictiveSafety].[SafetyPrediction] AS t
    INNER JOIN inserted AS i
        ON t.[CreatedUtc] = i.[CreatedUtc] AND t.[SafetyPredictionId] = i.[SafetyPredictionId];
END
GO



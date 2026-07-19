USE [PlantSafety.PredictiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        53-predictivedb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for PredictiveDB's 2 tables, plus one cross-table
--                reporting view over the real FK_PredictiveSafety_PredictionEvidence_
--                SafetyPrediction relationship.
--
-- Deliberately NOT created:
--   - No per-row join view (SafetyPrediction 1:many PredictionEvidence would duplicate
--     the prediction row per evidence row, which is not "deterministic reporting", it's
--     a raw join better left to the consumer); instead an aggregated
--     vw_SafetyPrediction_WithEvidenceCount is provided (1 row per prediction, matching
--     view-determinism rules -- no fan-out).
--   - No materialized summary table: only 2 tables and no documented time-series
--     aggregation need beyond per-prediction evidence counting (already served by the
--     view above without materialization). RECOMMENDED optional summary if/when a
--     model-performance dashboard is scoped: PredictiveSafety.SafetyPredictionSummary
--     (SummaryDateKey, ModelName, ModelVersion, Status, PredictionCount, AvgConfidenceScore)
--     -- not built here, no consumer grounded in memory yet.
--   - No tenant-scoped view: PredictiveDB tables carry no TenantId column at all
--     (SafetyPrediction/PredictionEvidence are keyed by Subject/SafetyPredictionId, not
--     tenant), so the CoreDB TVF pattern does not apply.
-- =============================================

CREATE OR ALTER VIEW PredictiveSafety.vw_SafetyPrediction_Active
AS
SELECT
    e.SafetyPredictionId, e.Subject, e.Status, e.ModelName, e.ModelVersion,
    e.ConfidenceScore, e.Explanation, e.HumanAgreed, e.ClosedAtUtc, e.RequestedByActorId,
    e.CorrelationId,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM PredictiveSafety.SafetyPrediction AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW PredictiveSafety.vw_PredictionEvidence_Active
AS
SELECT
    e.PredictionEvidenceId, e.SafetyPredictionId, e.EvidenceType, e.Description,
    e.Explanation, e.Weight, e.RecordedAtUtc,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM PredictiveSafety.PredictionEvidence AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          PredictiveSafety.vw_SafetyPrediction_WithEvidenceCount
-- Purpose:       Active SafetyPrediction records with an aggregated count/average-weight
--                of their active PredictionEvidence rows, via the real
--                FK_PredictiveSafety_PredictionEvidence_SafetyPrediction relationship.
--                One row per prediction (no fan-out), deterministic aggregate only.
-- =============================================
CREATE OR ALTER VIEW PredictiveSafety.vw_SafetyPrediction_WithEvidenceCount
AS
SELECT
    p.SafetyPredictionId, p.Subject, p.Status, p.ModelName, p.ModelVersion,
    p.ConfidenceScore, p.HumanAgreed, p.ClosedAtUtc,
    ISNULL(ev.EvidenceCount, 0) AS EvidenceCount,
    ev.AverageEvidenceWeight,
    p.CreatedUtc, p.CreatedBy, p.UpdatedUtc, p.UpdatedBy, p.IsDeleted
FROM PredictiveSafety.SafetyPrediction AS p
OUTER APPLY (
    SELECT COUNT(*) AS EvidenceCount, AVG(e.Weight) AS AverageEvidenceWeight
    FROM PredictiveSafety.PredictionEvidence AS e
    WHERE e.SafetyPredictionId = p.SafetyPredictionId
      AND ISNULL(e.IsDeleted, 0) = 0
) AS ev
WHERE ISNULL(p.IsDeleted, 0) = 0;
GO

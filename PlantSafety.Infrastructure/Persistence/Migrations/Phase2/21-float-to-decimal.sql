USE [PlantSafety.PredictiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        21-float-to-decimal.sql
-- Purpose:       Convert float columns holding bounded probability/weight/confidence
--                values to fixed-precision DECIMAL, eliminating binary floating-point
--                rounding drift for values that participate in safety-threshold
--                comparisons (see Intelligence Domain Safety Rule 4.4/4.5).
--
-- Discovery:     Live scan of INFORMATION_SCHEMA.COLUMNS WHERE DATA_TYPE = 'float'
--                across all 8 in-scope databases (CoreDB, ArchiveDB, CloudIdentityDB,
--                PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB)
--                on 2026-07-18 found exactly TWO float columns, both in PredictiveDB:
--
--   1. PredictiveSafety.SafetyPrediction.ConfidenceScore  (float, NULL)
--      -> Domain: Intelligence.Domain.PredictiveSafety.ValueObjects.ConfidenceScore.Value
--         (double), explicitly documented range [0.0, 1.0], compared against
--         IntelligenceConstants.MinimumConfidenceScore (0.70, Safety Rule 4.4) and
--         formatted "F4" (4 decimal places) in ToString(). A bounded, 4-decimal-place
--         probability -> DECIMAL(5,4) (range -9.9999..9.9999, comfortably covers
--         0.0000-1.0000 while matching the domain's own F4 formatting precision).
--         Nullability preserved (column is nullable in the live schema).
--
--   2. PredictiveSafety.PredictionEvidence.Weight  (float, NOT NULL)
--      -> Domain: Intelligence.Domain.PredictiveSafety.Entities.PredictionEvidence.
--         FeatureContributionWeight (double), explicitly documented as SHAP-compatible,
--         range [-1.0, 1.0] (signed -- positive increases risk, negative reduces it).
--         DECIMAL precision does not include the sign, so DECIMAL(5,4) (range
--         -9.9999..9.9999) is the correct precision/scale to use here too, keeping the
--         same fixed-point granularity as ConfidenceScore for consistency across the
--         PredictiveSafety schema. Nullability preserved (column is NOT NULL live).
--
-- No other float columns were found anywhere in the 8-database scope, so no further
-- conversions or "left alone" float columns exist to report for this pass.
--
-- Idempotent:    Guarded by INFORMATION_SCHEMA.COLUMNS DATA_TYPE check; safe to re-run.
-- =============================================

-- NOTE: both target columns are carried as INCLUDE columns on a live nonclustered
-- index (discovered live via sys.indexes/sys.index_columns), which blocks a direct
-- ALTER COLUMN. Each conversion below drops and rebuilds its dependent index around
-- the type change, using the index's real live definition -- not assumed/guessed.

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'PredictiveSafety'
      AND TABLE_NAME = 'SafetyPrediction'
      AND COLUMN_NAME = 'ConfidenceScore'
      AND DATA_TYPE = 'float'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyPrediction_Status'
               AND object_id = OBJECT_ID('PredictiveSafety.SafetyPrediction'))
        DROP INDEX IX_SafetyPrediction_Status ON [PredictiveSafety].[SafetyPrediction];

    ALTER TABLE [PredictiveSafety].[SafetyPrediction]
        ALTER COLUMN [ConfidenceScore] DECIMAL(5,4) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SafetyPrediction_Status'
                   AND object_id = OBJECT_ID('PredictiveSafety.SafetyPrediction'))
        CREATE NONCLUSTERED INDEX IX_SafetyPrediction_Status
            ON [PredictiveSafety].[SafetyPrediction] (Status)
            INCLUDE (Subject, ConfidenceScore, ClosedAtUtc);
END;
GO

IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'PredictiveSafety'
      AND TABLE_NAME = 'PredictionEvidence'
      AND COLUMN_NAME = 'Weight'
      AND DATA_TYPE = 'float'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PredictionEvidence_SafetyPredictionId'
               AND object_id = OBJECT_ID('PredictiveSafety.PredictionEvidence'))
        DROP INDEX IX_PredictionEvidence_SafetyPredictionId ON [PredictiveSafety].[PredictionEvidence];

    ALTER TABLE [PredictiveSafety].[PredictionEvidence]
        ALTER COLUMN [Weight] DECIMAL(5,4) NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PredictionEvidence_SafetyPredictionId'
                   AND object_id = OBJECT_ID('PredictiveSafety.PredictionEvidence'))
        CREATE NONCLUSTERED INDEX IX_PredictionEvidence_SafetyPredictionId
            ON [PredictiveSafety].[PredictionEvidence] (SafetyPredictionId)
            INCLUDE (EvidenceType, Weight);
END;
GO

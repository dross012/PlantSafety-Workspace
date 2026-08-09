-- ============================================================================
--  Project:        AI.PredictiveOptimization -- Phase1-PredictiveOptimizationLog
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PredictiveOptimizationLog_RecommendedFocus_GeneratedAtUtc' AND object_id = OBJECT_ID('AIPredictiveOptimization.PredictiveOptimizationLog'))
    CREATE NONCLUSTERED INDEX IX_PredictiveOptimizationLog_RecommendedFocus_GeneratedAtUtc
        ON [AIPredictiveOptimization].[PredictiveOptimizationLog] ([RecommendedFocus], [GeneratedAtUtc] DESC);
GO

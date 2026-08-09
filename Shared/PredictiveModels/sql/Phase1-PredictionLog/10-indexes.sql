-- ============================================================================
--  Project:        Shared.PredictiveModels -- Phase1-PredictionLog
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PredictionRunLog_ModelKind_SubjectId_RunAtUtc' AND object_id = OBJECT_ID('PredictiveModels.PredictionRunLog'))
    CREATE NONCLUSTERED INDEX IX_PredictionRunLog_ModelKind_SubjectId_RunAtUtc
        ON [PredictiveModels].[PredictionRunLog] ([ModelKind], [SubjectId], [RunAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PredictionRunLog_IsAnomalous' AND object_id = OBJECT_ID('PredictiveModels.PredictionRunLog'))
    CREATE NONCLUSTERED INDEX IX_PredictionRunLog_IsAnomalous
        ON [PredictiveModels].[PredictionRunLog] ([IsAnomalous]) WHERE [IsAnomalous] = 1;
GO

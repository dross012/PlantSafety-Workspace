-- ============================================================================
--  Project:        Enterprise.PredictiveModeling -- Phase1-PredictiveModeling
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ForecastLog_ForecastKind_GeneratedAtUtc' AND object_id = OBJECT_ID('EnterpriseForecast.ForecastLog'))
    CREATE NONCLUSTERED INDEX IX_ForecastLog_ForecastKind_GeneratedAtUtc
        ON [EnterpriseForecast].[ForecastLog] ([ForecastKind], [GeneratedAtUtc] DESC);
GO

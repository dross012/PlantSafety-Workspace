-- ============================================================================
--  Project:        Enterprise.PredictiveModeling
--  Module:         Phase1-PredictiveModeling
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EnterpriseForecast] schema -- durable
--                  logging for Enterprise.PredictiveModeling.Abstractions.
--                  Forecasts (Phase D.8: EnterpriseHazardForecast/
--                  ComplianceForecast/SeverityForecast/RiskForecast),
--                  currently pure function outputs with no persistence.
--                  Mirrors Shared.PredictiveModels' Phase1-PredictionLog
--                  shared-table design: one log table, ForecastKind
--                  discriminates.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseForecast')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseForecast] AUTHORIZATION dbo;');
END
GO

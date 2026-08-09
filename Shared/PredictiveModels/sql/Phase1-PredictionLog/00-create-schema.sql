-- ============================================================================
--  Project:        Shared.PredictiveModels
--  Module:         Phase1-PredictionLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [PredictiveModels] schema -- durable logging
--                  for Shared.PredictiveModels.Abstractions.Models' five
--                  pure deterministic prediction functions (Phase C.11):
--                  HazardLikelihoodModel, SeverityLikelihoodModel,
--                  ExposureLikelihoodModel, MachineFailurePredictionModel,
--                  EmployeeInjuryPredictionModel -- all currently in-memory
--                  only. One log table per model kind, sharing a common
--                  scalar shape (moving average/trend slope/z-score/
--                  prediction/anomaly-flag are the union of every model's
--                  output fields; each model only populates the columns its
--                  own output record has).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PredictiveModels')
BEGIN
    EXEC('CREATE SCHEMA [PredictiveModels] AUTHORIZATION dbo;');
END
GO

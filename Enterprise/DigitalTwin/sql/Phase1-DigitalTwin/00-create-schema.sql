-- ============================================================================
--  Project:        Enterprise.DigitalTwin
--  Module:         Phase1-DigitalTwin
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [DigitalTwin] schema -- durable snapshot
--                  logging for Enterprise.DigitalTwin.Abstractions.Twins
--                  (Phase D.7: DigitalTwinFacility/Employee/Machine/Hazard/
--                  Workflow/Event), currently pure simulation with no
--                  persisted snapshots. Per this task's instruction: a
--                  session/log table, not a fully normalized relational
--                  model of every twin type -- each twin kind's scalar
--                  snapshot is serialized as its own JSON column on one
--                  shared TwinSnapshot row, since a simulation run
--                  typically snapshots several twin kinds together as one
--                  "what would happen if" scenario.
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin] AUTHORIZATION dbo;');
END
GO

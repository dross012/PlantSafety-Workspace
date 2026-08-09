-- ============================================================================
--  Project:        Shared.MachineDomain
--  Module:         Phase1-Machine
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [MachineDomain] schema -- the SQL surface
--                  for Shared.MachineDomain.Abstractions.Aggregates.
--                  MachineAggregate/MachineState (Path C.2): location,
--                  maintenance profile, failure records, safety score.
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'MachineDomain')
BEGIN
    EXEC('CREATE SCHEMA [MachineDomain] AUTHORIZATION dbo;');
END
GO

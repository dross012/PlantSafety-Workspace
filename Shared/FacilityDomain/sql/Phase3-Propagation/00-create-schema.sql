-- ============================================================================
--  Project:        Shared.FacilityDomain
--  Module:         Phase3-Propagation
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [Propagation] schema -- the durable, append-
--                  only mirror of Phase C.6's HazardPropagationEvent and
--                  Phase C.9's HazardChain/HazardChainNode. Kept separate
--                  from [FacilityDomain] (Phase2) because these are
--                  event-history-shaped facts about what happened, not
--                  current-state aggregate snapshots.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Phase1-Lookup's note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Propagation')
BEGIN
    EXEC('CREATE SCHEMA [Propagation] AUTHORIZATION dbo;');
END
GO

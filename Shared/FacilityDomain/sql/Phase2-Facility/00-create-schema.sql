-- ============================================================================
--  Project:        Shared.FacilityDomain
--  Module:         Phase2-Facility
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2027.1
--  Description:    Creates the [FacilityDomain] schema -- the SQL surface for
--                  Shared.FacilityDomain.Abstractions.Aggregates.
--                  FacilityAggregate/FacilityState (Path B) and its Path C
--                  extensions: three-level Building/Area/Zone hierarchy
--                  (Phase C.5) and lifecycle status (Phase C.12). Hazard
--                  propagation (Phase C.6) and multi-hazard chains
--                  (Phase C.9) are event-history-shaped and live in their own
--                  Phase3-Propagation folder/schema instead (append-only,
--                  not part of the current-state aggregate snapshot).
--
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Phase1-Lookup\
--  00-create-schema.sql's DATABASE PLACEMENT ASSUMPTION note.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'FacilityDomain')
BEGIN
    EXEC('CREATE SCHEMA [FacilityDomain] AUTHORIZATION dbo;');
END
GO

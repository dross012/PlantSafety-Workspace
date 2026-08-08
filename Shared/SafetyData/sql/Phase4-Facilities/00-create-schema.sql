-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase4-Facilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Creates the [Facilities] schema -- the SQL surface for
--                  Shared.SafetyData.Abstractions.Models.Industries.Facilities
--                  (FacilityProfile, FacilityComplianceProfile). Mirrors
--                  [SafetyTaxonomy] (Phase2) and [Orchestration] (Phase3)'s
--                  precedent of one schema per owning C# namespace segment.
--
--  CONCURRENT WORK NOTE: see Phase3-Orchestration\00-create-schema.sql's
--  CONCURRENT WORK NOTE -- identical rationale applies here. This schema and
--  its tables ([Facilities].[FacilityProfile],
--  [Facilities].[FacilityComplianceProfile]) are being authored by the
--  concurrently-running schema-sentinel-agent; this folder's indexes/
--  procedures/views were authored independently against the assumed
--  Handbook-conventional naming and column shape documented in each object's
--  own header.
--
--  SCOPE NOTE (mirrors FacilityProfile.cs's own SCOPE NOTE): FacilityId/
--  Name/Location are caller-supplied inputs, not owned/persisted identity in
--  the C# layer -- Shared.SafetyData deliberately does not define a
--  persisted Facility aggregate. These tables exist purely as the SQL
--  read-model cache of FacilityProfileBuilder.Build's most recent output for
--  a given FacilityId (upserted whenever it is recomputed), not as the
--  authoritative Facility identity store -- that distinction matters for any
--  future FK design against a real PlantSafety Facility/Site aggregate,
--  should one be built.
--
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], per the canonical
--  database-per-solution inventory.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Facilities')
BEGIN
    EXEC('CREATE SCHEMA [Facilities] AUTHORIZATION dbo;');
END
GO

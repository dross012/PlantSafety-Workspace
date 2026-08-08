-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Creates the [ComplianceRisk] schema -- the SQL surface
--                  for Shared.SafetyData.Abstractions.Models.Industries.
--                  ComplianceRisk (ComplianceScore, RiskScore,
--                  ComplianceReadiness, RiskReadiness). Mirrors
--                  [SafetyTaxonomy] (Phase2), [Orchestration] (Phase3), and
--                  [Facilities] (Phase4)'s precedent of one schema per
--                  owning C# namespace segment.
--
--  CONCURRENT WORK NOTE: see Phase3-Orchestration\00-create-schema.sql's
--  CONCURRENT WORK NOTE -- identical rationale applies here. This schema and
--  its tables ([ComplianceRisk].[ComplianceScore],
--  [ComplianceRisk].[RiskScore], [ComplianceRisk].[ComplianceReadiness],
--  [ComplianceRisk].[RiskReadiness]) are being authored by the
--  concurrently-running schema-sentinel-agent; this folder's indexes/
--  procedures/views were authored independently against the assumed
--  Handbook-conventional naming and column shape documented in each object's
--  own header.
--
--  SHAPE ASSUMPTION: each of the four tables is treated as an append-only
--  time series keyed by (FacilityId, ComputedAtUtc) -- one row per
--  recomputation of that facility's score/readiness (ComplianceScoreBuilder.
--  Build / RiskScoreBuilder.Build / ComplianceReadinessBuilder.Build /
--  RiskReadinessBuilder.Build are all pure functions of the facility's
--  current FacilityProfile, so a fresh row is expected every time the
--  orchestration layer recomputes it in response to a real event) rather
--  than a single upserted current-state row -- this matches this folder's
--  task brief's explicit ask for a "latest compliance/risk snapshot per
--  facility" VIEW (which is only a meaningful, non-trivial object to build
--  if there can be more than one historical row per facility; a single
--  current-state row per facility would make such a view a trivial 1:1
--  passthrough, as with [Facilities] in Phase4).
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

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ComplianceRisk')
BEGIN
    EXEC('CREATE SCHEMA [ComplianceRisk] AUTHORIZATION dbo;');
END
GO

-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Creates the [Orchestration] schema -- the SQL surface for
--                  Shared.SafetyData.Abstractions.Models.Industries.Orchestration
--                  (FacilityEventOrchestrationResult, TenantFacilityRegistration)
--                  and the AccidentManagement-/RiskManagement-specific
--                  orchestration read models in Reporting.Infrastructure.
--                  EventBus.ReadModels (AccidentImpactEntry, RiskImpactEntry),
--                  which are additive event-history variants of the same
--                  FacilityEventOrchestrationResult shape.
--
--  CONCURRENT WORK NOTE: this schema and the tables it contains
--  ([Orchestration].[FacilityEventOrchestrationResult],
--  [Orchestration].[TenantFacilityRegistration],
--  [Orchestration].[AccidentImpactEntry], [Orchestration].[RiskImpactEntry])
--  are being authored by the concurrently-running schema-sentinel-agent per
--  the Handbook's deterministic naming convention (table name = the C#
--  record's own name, PascalCase; schema name = the owning C# namespace
--  segment -- "Orchestration" in this case, mirroring
--  Shared.SafetyData.Abstractions.Models.Industries.Orchestration exactly,
--  the same pattern [SafetyTaxonomy] already establishes for the
--  Industries.* namespace segment). This file and the rest of this folder
--  (indexes/procedures/views/functions) were authored independently against
--  that same assumed naming convention -- see each object's own header for
--  the exact column-shape assumption it depends on if the schema-sentinel
--  agent's actual DDL differs.
--
--  NAMING / COLLISION NOTE: [Orchestration] is deliberately NOT [dbo] and
--  deliberately NOT reused from [SafetyTaxonomy] (Phase2), because this data
--  is mutable, tenant-scoped, event-sourced read-model data (recomputed on
--  every real AccidentManagement/RiskManagement event), fundamentally
--  different from [SafetyTaxonomy]'s static, seeded, read-only reference
--  data -- the two must never be confused or joined by accident.
--
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], the same physical database
--  already used by Shared.SafetyData's Phase1/Phase2 scripts, per the
--  canonical database-per-solution inventory in
--  sql-database-skill/references/00-canonical-templates.md.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Orchestration')
BEGIN
    EXEC('CREATE SCHEMA [Orchestration] AUTHORIZATION dbo;');
END
GO

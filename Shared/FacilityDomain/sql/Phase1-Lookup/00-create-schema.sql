-- ============================================================================
--  Project:        Shared.FacilityDomain
--  Module:         Phase1-Lookup
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2027.1
--  Description:    Creates the [Lookup] schema -- the SINGLE canonical home
--                  for the three shared, additive-only classification
--                  vocabularies reused verbatim across nearly every Path
--                  B/C/D/E bounded context: Shared.FacilityDomain.
--                  Abstractions.Severity.SeverityLevel (Minor/Moderate/
--                  Severe/Catastrophic), .Hazards.HazardType (9-member enum),
--                  and .Regulatory.RegulatoryJurisdiction (OSHA/EPA/DOT/NFPA/
--                  State/Local). Per this task's explicit instruction: ONE
--                  canonical lookup table per shared enum, referenced by FK
--                  from every other table that uses it -- not a duplicated
--                  CHECK constraint per table.
--
--  DATABASE PLACEMENT ASSUMPTION: [PlantSafety.CoreDB]. No dedicated database
--  exists in 00-canonical-templates.md's live inventory for
--  Shared.FacilityDomain/EmployeeDomain/MachineDomain/WorkflowEngine/
--  PredictiveModels (all new Path B/C bounded contexts). CoreDB already hosts
--  Shared.SafetyData's [Facilities]/[Orchestration]/[ComplianceRisk]/
--  [SafetyTaxonomy] schemas under the established "one schema per owning C#
--  namespace segment, shared CoreDB" convention, and FacilityDomain is the
--  closest-related, most foundational of the new contexts (Facilities.
--  FacilityProfile already FKs into [SafetyTaxonomy].[Industry] in the same
--  database) -- so every new Path B/C/D/E schema in this task uses CoreDB
--  too, rather than provisioning N new single-purpose databases without a
--  human decision. Flag for a dedicated-database provisioning decision if
--  these bounded contexts grow to independent production scale.
--
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Lookup')
BEGIN
    EXEC('CREATE SCHEMA [Lookup] AUTHORIZATION dbo;');
END
GO

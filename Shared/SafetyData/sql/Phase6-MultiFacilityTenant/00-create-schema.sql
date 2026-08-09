-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase6-MultiFacilityTenant
--  Layer:          Infrastructure - SQL Scripts
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [TenantMap] schema -- Path C's "multi-facility
--                  tenant map" extension. [Orchestration].[TenantFacilityRegistration]
--                  (Phase3) is a strict 1 tenant : 1 facility binding (its own
--                  header: "UQ_TenantFacilityRegistration_FacilityId ...
--                  preserve the 1 tenant : 1 facility binding"). This phase
--                  adds the genuinely NEW many-facilities-per-tenant mapping
--                  Path C's task brief calls for, as an additive schema
--                  rather than altering Phase3's existing 1:1 table (which
--                  real callers may still depend on).
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], same database as every other
--  Shared.SafetyData schema (Facilities/Orchestration/ComplianceRisk/
--  SafetyTaxonomy).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'TenantMap')
BEGIN
    EXEC('CREATE SCHEMA [TenantMap] AUTHORIZATION dbo;');
END
GO

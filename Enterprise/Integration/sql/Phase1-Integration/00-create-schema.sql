-- ============================================================================
--  Project:        Enterprise.Integration
--  Module:         Phase1-Integration
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EnterpriseIntegration] schema -- durable
--                  storage for Enterprise.Integration.Mapping.
--                  EnterpriseIntegrationMapping (field-mapping
--                  definitions, currently in-memory only) and durable
--                  logging of ingested EnterpriseIntegrationEvent (Phase
--                  D.9), one row per real inbound event from an external
--                  HR/ERP/MES/CMMS/EHS system.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note. NOTE: distinct from the
--  canonical inventory's already-provisioned [PlantSafety.IntegrationDB]
--  (a *different*, pre-existing integrations concept in this repo, per
--  00-canonical-templates.md); this is Enterprise.Integration (Path D.9),
--  a separate new bounded context, not a consumer of that database.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseIntegration')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseIntegration] AUTHORIZATION dbo;');
END
GO

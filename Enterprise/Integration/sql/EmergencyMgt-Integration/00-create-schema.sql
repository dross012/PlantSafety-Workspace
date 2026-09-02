-- ============================================================================
--  Project:        Enterprise.Integration
--  Module:         EmergencyMgt-Integration
--  File:           00-create-schema.sql
--  Created:        2026-08-28
--  Description:    Creates the [EmergencyMgtIntegration] schema -- PlantSafety-side durable
--                  storage for the EmergencyMgt <-> PlantSafety integration surface, per
--                  "EmergencyMgt Extensions.docx" / the EmergencyMgt -> PlantSafety integration
--                  request. Mirrors IncidentMgt-Integration's own pattern exactly (see that
--                  folder's 00-create-schema.sql), which is itself the only prior instance of
--                  this cross-module integration shape in this program -- both are source-only
--                  as of this file (neither had been deployed to the live PlantSafety.CoreDB
--                  before this pass; EmergencyMgt-Integration is deployed live first).
--  Architecture:   EmergencyMgt is a physically separate database-per-solution deployment (see
--                  EmergencyMgt repo, source/database/EmergencyManagementDB). It is NOT
--                  linked-server-joined to PlantSafety.CoreDB. Integration is one-directional
--                  push, eventually-consistent: the EmergencyMgt Servicer pushes summary/
--                  lifecycle/corrective-action/readiness rows and governance signals into this
--                  schema via usp_*_Upsert / usp_*_Record procedures; PlantSafety reads them
--                  back out through the read-only views in 30-views.sql. No PlantSafety code
--                  queries EmergencyMgt's database directly, and no automatic puller exists yet
--                  on the EmergencyMgt side (EmergencyMgt's own Governance.
--                  IntegrationContractRegistration records PlantSafety as DeferredSeamOnly --
--                  see EmergencyMgt docs/deviations.md DEV-022) -- this schema is the receiving
--                  surface a future EmergencyMgt-side pusher would write into, not yet a wired,
--                  automatic feed.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], matching IncidentMgt-Integration's placement.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EmergencyMgtIntegration')
BEGIN
    EXEC('CREATE SCHEMA [EmergencyMgtIntegration] AUTHORIZATION dbo;');
END
GO

-- ============================================================================
--  Project:        Enterprise.Integration
--  Module:         IncidentMgt-Integration
--  File:           00-create-schema.sql
--  Created:        2026-08-28
--  Description:    Creates the [IncidentMgtIntegration] schema -- PlantSafety-side durable
--                  storage for the IncidentMgt <-> PlantSafety integration surface, per
--                  "IncidentMgt Removal from PlantSafety.docx" Section 3 (INTEGRATE INCIDENTMGT
--                  BACK INTO PLANTSAFETY).
--  Architecture:   IncidentMgt is a physically separate database-per-solution deployment (see
--                  IncidentMgt repo, sql/IncidentMgt/Migrations/000_CreateDatabase.sql). It is
--                  NOT linked-server-joined to PlantSafety.CoreDB. Integration is one-directional
--                  push, eventually-consistent, following the same IntegrationEventLog precedent
--                  as Phase1-Integration (see that folder's 20-usp-integration.sql,
--                  usp_IntegrationEventLog_Record): the IncidentMgt Servicer pushes summary rows
--                  and governance signals into this schema via usp_*_Upsert / usp_*_Record
--                  procedures; PlantSafety reads them back out through the read-only views in
--                  30-views.sql. No PlantSafety code queries IncidentMgt's database directly.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB], matching Phase1-Integration's placement.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'IncidentMgtIntegration')
BEGIN
    EXEC('CREATE SCHEMA [IncidentMgtIntegration] AUTHORIZATION dbo;');
END
GO

-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase4-Facilities
--  Layer:          Infrastructure - SQL Scripts
--  File:           10-indexes.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Secondary indexes for [Facilities] tables. These are NOT
--                  event-history-shaped tables (no TenantId, no
--                  OccurredAtUtc column -- FacilityProfile/
--                  FacilityComplianceProfile are current-state snapshot rows,
--                  one per FacilityId, upserted whenever recomputed), so the
--                  (TenantId)/(OccurredAtUtc) covering-index requirement in
--                  the task brief does not apply here -- it is scoped
--                  explicitly to "every event-history-shaped table"
--                  ([Orchestration]'s FacilityEventOrchestrationResult/
--                  AccidentImpactEntry/RiskImpactEntry, see
--                  Phase3-Orchestration\10-indexes.sql). The clustered PK on
--                  FacilityId (declared on each table's own CREATE TABLE per
--                  the schema-sentinel agent's DDL) already covers every
--                  read path FacilityProfileBuilder-derived callers need
--                  (point lookup by FacilityId).
--
--  ASSUMED COLUMN SHAPE: see 00-create-schema.sql's CONCURRENT WORK NOTE.
--
--  Idempotent: guarded by sys.indexes existence checks. Safe to re-run.
--  Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

------------------------------------------------------------
-- Facilities.FacilityProfile
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityProfile_IndustryName' AND object_id = OBJECT_ID('Facilities.FacilityProfile'))
    CREATE NONCLUSTERED INDEX IX_FacilityProfile_IndustryName
        ON [Facilities].[FacilityProfile] ([IndustryName]);
GO

------------------------------------------------------------
-- Facilities.FacilityComplianceProfile
-- (FacilityId is both PK and FK back to FacilityProfile per the
--  schema-sentinel agent's DDL; the clustered PK already covers the 1:1
--  join used by 35-view-facilityprofile-compliancesummary.sql, so no
--  additional index is required.)
------------------------------------------------------------

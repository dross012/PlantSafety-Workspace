-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase5-ComplianceRisk
--  Layer:          Infrastructure - SQL Scripts
--  File:           10-indexes.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Covering indexes for [ComplianceRisk]'s four
--                  append-only score/readiness history tables, supporting
--                  the "most recent row for this facility" read path every
--                  GetLatestByFacility procedure and the combined snapshot
--                  view in this folder use. FacilityId is not a TenantId
--                  (these tables have no TenantId column -- they are scoped
--                  by FacilityId, one level below tenant, per
--                  ComplianceScore/RiskScore/ComplianceReadiness/
--                  RiskReadiness's own C# record shape), so the covering
--                  index here is (FacilityId, ComputedAtUtc DESC) rather
--                  than the (TenantId)/(OccurredAtUtc) pair used for
--                  [Orchestration]'s tenant-scoped event-history tables.
--
--  ASSUMED COLUMN SHAPE: see 00-create-schema.sql's SHAPE ASSUMPTION note.
--  Composite PK (FacilityId, ComputedAtUtc) is declared on each table's own
--  CREATE TABLE per the schema-sentinel agent's DDL.
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
-- ComplianceRisk.ComplianceScore
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ComplianceScore_FacilityId_ComputedAtUtc' AND object_id = OBJECT_ID('ComplianceRisk.ComplianceScore'))
    CREATE NONCLUSTERED INDEX IX_ComplianceScore_FacilityId_ComputedAtUtc
        ON [ComplianceRisk].[ComplianceScore] ([FacilityId], [ComputedAtUtc] DESC);
GO

------------------------------------------------------------
-- ComplianceRisk.RiskScore
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskScore_FacilityId_ComputedAtUtc' AND object_id = OBJECT_ID('ComplianceRisk.RiskScore'))
    CREATE NONCLUSTERED INDEX IX_RiskScore_FacilityId_ComputedAtUtc
        ON [ComplianceRisk].[RiskScore] ([FacilityId], [ComputedAtUtc] DESC);
GO

------------------------------------------------------------
-- ComplianceRisk.ComplianceReadiness
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ComplianceReadiness_FacilityId_ComputedAtUtc' AND object_id = OBJECT_ID('ComplianceRisk.ComplianceReadiness'))
    CREATE NONCLUSTERED INDEX IX_ComplianceReadiness_FacilityId_ComputedAtUtc
        ON [ComplianceRisk].[ComplianceReadiness] ([FacilityId], [ComputedAtUtc] DESC);
GO

------------------------------------------------------------
-- ComplianceRisk.RiskReadiness
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskReadiness_FacilityId_ComputedAtUtc' AND object_id = OBJECT_ID('ComplianceRisk.RiskReadiness'))
    CREATE NONCLUSTERED INDEX IX_RiskReadiness_FacilityId_ComputedAtUtc
        ON [ComplianceRisk].[RiskReadiness] ([FacilityId], [ComputedAtUtc] DESC);
GO

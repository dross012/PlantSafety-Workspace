-- ============================================================================
--  Project:        Shared.SafetyData
--  Module:         Phase3-Orchestration
--  Layer:          Infrastructure - SQL Scripts
--  File:           10-indexes.sql
--  Created:        2026-08-08
--  Author:         Platform Team (SQL & Database Agent)
--  SchemaVersion:  2026.1
--  Description:    Covering indexes for every event-history-shaped table in
--                  [Orchestration], per the two filters every C# store query
--                  already uses (Reporting.Infrastructure.EventBus.ReadModels
--                  InMemory*Store.GetRecent(tenantId, maxResults) /
--                  GetByAccidentId / GetByRiskId):
--                    - (TenantId) -- GetRecent's optional tenant filter
--                    - (OccurredAtUtc DESC) -- GetRecent's ordering
--                    - (AccidentId) / (RiskId) -- GetByAccidentId/GetByRiskId
--                  PK indexes (EventId, StrongId-safe GUID identity matching
--                  the C# stores' Record(Guid eventId, ...) key) are declared
--                  on each table's own CREATE TABLE per the schema-sentinel
--                  agent's DDL; this file only adds the secondary,
--                  read-path-driven nonclustered indexes.
--
--  ASSUMED COLUMN SHAPE (see 00-create-schema.sql's CONCURRENT WORK NOTE):
--  every table below carries at minimum EventId (PK), TenantId, OccurredAtUtc,
--  and (for AccidentImpactEntry/RiskImpactEntry) AccidentId/RiskId respectively.
--  If the schema-sentinel agent's actual DDL uses different column names,
--  these CREATE INDEX statements will fail loudly at deploy time (IF NOT
--  EXISTS guards only skip re-creation of an index that already exists by
--  name, they do not silently tolerate a missing column) rather than
--  silently drifting -- re-run this file once both script sets are applied
--  in order.
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
-- Orchestration.FacilityEventOrchestrationResult
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityEventOrchestrationResult_TenantId' AND object_id = OBJECT_ID('Orchestration.FacilityEventOrchestrationResult'))
    CREATE NONCLUSTERED INDEX IX_FacilityEventOrchestrationResult_TenantId
        ON [Orchestration].[FacilityEventOrchestrationResult] ([TenantId])
        INCLUDE ([FacilityId], [OccurredAtUtc], [Category], [RequiresImmediateAttention]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityEventOrchestrationResult_OccurredAtUtc' AND object_id = OBJECT_ID('Orchestration.FacilityEventOrchestrationResult'))
    CREATE NONCLUSTERED INDEX IX_FacilityEventOrchestrationResult_OccurredAtUtc
        ON [Orchestration].[FacilityEventOrchestrationResult] ([OccurredAtUtc] DESC)
        INCLUDE ([TenantId], [FacilityId]);
GO

------------------------------------------------------------
-- Orchestration.TenantFacilityRegistration
-- (current-state registry, not event-history: keyed 1:1 by TenantId, so the
--  clustered PK on TenantId already covers TryResolve/Unregister; no
--  additional secondary index is needed for GetAll, which scans the whole,
--  intentionally small table.)
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TenantFacilityRegistration_FacilityId' AND object_id = OBJECT_ID('Orchestration.TenantFacilityRegistration'))
    CREATE NONCLUSTERED INDEX IX_TenantFacilityRegistration_FacilityId
        ON [Orchestration].[TenantFacilityRegistration] ([FacilityId]);
GO

------------------------------------------------------------
-- Orchestration.AccidentImpactEntry
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccidentImpactEntry_TenantId' AND object_id = OBJECT_ID('Orchestration.AccidentImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_AccidentImpactEntry_TenantId
        ON [Orchestration].[AccidentImpactEntry] ([TenantId])
        INCLUDE ([AccidentId], [FacilityId], [OccurredAtUtc]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccidentImpactEntry_OccurredAtUtc' AND object_id = OBJECT_ID('Orchestration.AccidentImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_AccidentImpactEntry_OccurredAtUtc
        ON [Orchestration].[AccidentImpactEntry] ([OccurredAtUtc] DESC)
        INCLUDE ([TenantId], [AccidentId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccidentImpactEntry_AccidentId' AND object_id = OBJECT_ID('Orchestration.AccidentImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_AccidentImpactEntry_AccidentId
        ON [Orchestration].[AccidentImpactEntry] ([AccidentId], [OccurredAtUtc]);
GO

------------------------------------------------------------
-- Orchestration.RiskImpactEntry
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskImpactEntry_TenantId' AND object_id = OBJECT_ID('Orchestration.RiskImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_RiskImpactEntry_TenantId
        ON [Orchestration].[RiskImpactEntry] ([TenantId])
        INCLUDE ([RiskId], [FacilityId], [OccurredAtUtc]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskImpactEntry_OccurredAtUtc' AND object_id = OBJECT_ID('Orchestration.RiskImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_RiskImpactEntry_OccurredAtUtc
        ON [Orchestration].[RiskImpactEntry] ([OccurredAtUtc] DESC)
        INCLUDE ([TenantId], [RiskId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RiskImpactEntry_RiskId' AND object_id = OBJECT_ID('Orchestration.RiskImpactEntry'))
    CREATE NONCLUSTERED INDEX IX_RiskImpactEntry_RiskId
        ON [Orchestration].[RiskImpactEntry] ([RiskId], [OccurredAtUtc]);
GO

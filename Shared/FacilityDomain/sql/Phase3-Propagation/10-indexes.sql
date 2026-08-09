-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase3-Propagation
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Description:    Covering indexes for time-range/facility-scoped queries
--                  on the two event-history-shaped tables.
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HazardPropagationEvent_FacilityId_OccurredAtUtc' AND object_id = OBJECT_ID('Propagation.HazardPropagationEvent'))
    CREATE NONCLUSTERED INDEX IX_HazardPropagationEvent_FacilityId_OccurredAtUtc
        ON [Propagation].[HazardPropagationEvent] ([FacilityId], [OccurredAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HazardChain_FacilityId_BuiltAtUtc' AND object_id = OBJECT_ID('Propagation.HazardChain'))
    CREATE NONCLUSTERED INDEX IX_HazardChain_FacilityId_BuiltAtUtc
        ON [Propagation].[HazardChain] ([FacilityId], [BuiltAtUtc] DESC);
GO

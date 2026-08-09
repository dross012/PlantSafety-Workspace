-- ============================================================================
--  Project:        Shared.FacilityDomain -- Phase2-Facility
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Description:    Secondary indexes for [FacilityDomain] tables --
--                  facility lookups by industry, hierarchy traversal by
--                  parent, and time-range queries on the append-only
--                  FacilityLifecycleEvent history.
--  Idempotent: guarded by sys.indexes existence checks. Safe to re-run.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Facility_IndustryName' AND object_id = OBJECT_ID('FacilityDomain.Facility'))
    CREATE NONCLUSTERED INDEX IX_Facility_IndustryName ON [FacilityDomain].[Facility] ([IndustryName]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Facility_LifecycleStatus' AND object_id = OBJECT_ID('FacilityDomain.Facility'))
    CREATE NONCLUSTERED INDEX IX_Facility_LifecycleStatus ON [FacilityDomain].[Facility] ([LifecycleStatus]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityBuilding_FacilityId' AND object_id = OBJECT_ID('FacilityDomain.FacilityBuilding'))
    CREATE NONCLUSTERED INDEX IX_FacilityBuilding_FacilityId ON [FacilityDomain].[FacilityBuilding] ([FacilityId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityArea_BuildingId' AND object_id = OBJECT_ID('FacilityDomain.FacilityArea'))
    CREATE NONCLUSTERED INDEX IX_FacilityArea_BuildingId ON [FacilityDomain].[FacilityArea] ([BuildingId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityZone_AreaId' AND object_id = OBJECT_ID('FacilityDomain.FacilityZone'))
    CREATE NONCLUSTERED INDEX IX_FacilityZone_AreaId ON [FacilityDomain].[FacilityZone] ([AreaId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityLifecycleEvent_FacilityId_OccurredAtUtc' AND object_id = OBJECT_ID('FacilityDomain.FacilityLifecycleEvent'))
    CREATE NONCLUSTERED INDEX IX_FacilityLifecycleEvent_FacilityId_OccurredAtUtc
        ON [FacilityDomain].[FacilityLifecycleEvent] ([FacilityId], [OccurredAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacilityHazardProfileItem_HazardTypeCode' AND object_id = OBJECT_ID('FacilityDomain.FacilityHazardProfileItem'))
    CREATE NONCLUSTERED INDEX IX_FacilityHazardProfileItem_HazardTypeCode
        ON [FacilityDomain].[FacilityHazardProfileItem] ([HazardTypeCode]);
GO

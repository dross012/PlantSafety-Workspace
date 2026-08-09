-- =============================================
-- Shared.FacilityDomain -- Phase2-Facility
-- Script:        35-view-facility-hierarchysummary.sql
-- Purpose:       FacilityDomain.vw_Facility_HierarchySummary -- one row per
--                Facility with Building/Area/Zone counts, mirroring the
--                "current row per facility" read-model shape used
--                throughout Shared.SafetyData's own views.
-- Determinism / soft-delete: no ORDER BY, explicit column list, IsDeleted
-- filtered on every joined table.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [FacilityDomain].[vw_Facility_HierarchySummary]
AS
SELECT
    f.[FacilityId],
    f.[Name]              AS [FacilityName],
    f.[IndustryName],
    f.[LifecycleStatus],
    COUNT(DISTINCT b.[BuildingId]) AS [BuildingCount],
    COUNT(DISTINCT a.[AreaId])     AS [AreaCount],
    COUNT(DISTINCT z.[ZoneId])     AS [ZoneCount],
    f.[UpdatedAtUtc]
FROM [FacilityDomain].[Facility] AS f
LEFT JOIN [FacilityDomain].[FacilityBuilding] AS b
    ON b.[FacilityId] = f.[FacilityId] AND ISNULL(b.[IsDeleted], 0) = 0
LEFT JOIN [FacilityDomain].[FacilityArea] AS a
    ON a.[BuildingId] = b.[BuildingId] AND ISNULL(a.[IsDeleted], 0) = 0
LEFT JOIN [FacilityDomain].[FacilityZone] AS z
    ON z.[AreaId] = a.[AreaId] AND ISNULL(z.[IsDeleted], 0) = 0
WHERE ISNULL(f.[IsDeleted], 0) = 0
GROUP BY f.[FacilityId], f.[Name], f.[IndustryName], f.[LifecycleStatus], f.[UpdatedAtUtc];
GO

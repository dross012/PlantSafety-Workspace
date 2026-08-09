-- =============================================
-- Shared.SafetyData -- Phase6-MultiFacilityTenant
-- Script:        35-view-tenantfacilitymap-summary.sql
-- Purpose:       TenantMap.vw_TenantFacilityMap_Summary -- per-tenant
--                facility counts and primary facility.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [TenantMap].[vw_TenantFacilityMap_Summary]
AS
SELECT
    m.[TenantId],
    COUNT(*) AS [FacilityCount],
    MAX(CASE WHEN m.[IsPrimaryFacility] = 1 THEN m.[FacilityId] END) AS [PrimaryFacilityId]
FROM [TenantMap].[TenantFacilityMap] AS m
WHERE ISNULL(m.[IsDeleted], 0) = 0
GROUP BY m.[TenantId];
GO

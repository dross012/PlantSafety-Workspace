-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FacilityTrafficMonitoring_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted FacilityTrafficMonitoring rows. Backs IFacilityTrafficMonitoringRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_FacilityTrafficMonitoring_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT FacilityTrafficMonitoringId, TenantId, SiteId, Status, ZoneCode, EventType, SeverityScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[FacilityTrafficMonitoring]
    WHERE IsDeleted = 0;
END
GO

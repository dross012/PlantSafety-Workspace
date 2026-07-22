-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FacilityTrafficMonitoring_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single FacilityTrafficMonitoring row by identity. Backs IFacilityTrafficMonitoringRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_FacilityTrafficMonitoring_GetById]
    @FacilityTrafficMonitoringId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT FacilityTrafficMonitoringId, TenantId, SiteId, Status, ZoneCode, EventType, SeverityScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[FacilityTrafficMonitoring]
    WHERE FacilityTrafficMonitoringId = @FacilityTrafficMonitoringId AND IsDeleted = 0;
END
GO

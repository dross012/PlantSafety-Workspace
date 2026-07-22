-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEBehavioralMonitoring_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PPEBehavioralMonitoring rows. Backs IPPEBehavioralMonitoringRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_PPEBehavioralMonitoring_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEBehavioralMonitoringId, TenantId, SiteId, Status, SensorId, ZoneCode, DetectedEventType, ConfidenceScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[PPEBehavioralMonitoring]
    WHERE IsDeleted = 0;
END
GO

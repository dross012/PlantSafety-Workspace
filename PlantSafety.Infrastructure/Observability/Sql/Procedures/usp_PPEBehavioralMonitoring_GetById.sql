-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEBehavioralMonitoring_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PPEBehavioralMonitoring row by identity. Backs IPPEBehavioralMonitoringRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_PPEBehavioralMonitoring_GetById]
    @PPEBehavioralMonitoringId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEBehavioralMonitoringId, TenantId, SiteId, Status, SensorId, ZoneCode, DetectedEventType, ConfidenceScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[PPEBehavioralMonitoring]
    WHERE PPEBehavioralMonitoringId = @PPEBehavioralMonitoringId AND IsDeleted = 0;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalAssetMonitoring_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single EnvironmentalAssetMonitoring row by identity. Backs IEnvironmentalAssetMonitoringRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_EnvironmentalAssetMonitoring_GetById]
    @EnvironmentalAssetMonitoringId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT EnvironmentalAssetMonitoringId, TenantId, SiteId, Status, SensorId, MeasurementType, Value, ThresholdExceeded, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[EnvironmentalAssetMonitoring]
    WHERE EnvironmentalAssetMonitoringId = @EnvironmentalAssetMonitoringId AND IsDeleted = 0;
END
GO

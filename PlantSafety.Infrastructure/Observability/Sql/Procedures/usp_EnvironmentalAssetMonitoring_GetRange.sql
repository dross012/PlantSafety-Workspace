-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalAssetMonitoring_GetRange.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IEnvironmentalAssetMonitoringRepository.GetEnvironmentalAssetMonitoringAsync
--                  (Application-layer read model). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_EnvironmentalAssetMonitoring_GetRange]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT EnvironmentalAssetMonitoringId, TenantId, SiteId, Status, SensorId, MeasurementType, Value, ThresholdExceeded, CreatedAtUtc
    FROM [Observability].[EnvironmentalAssetMonitoring]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO

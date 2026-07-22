-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalAssetMonitoring_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EnvironmentalAssetMonitoring rows. Backs IEnvironmentalAssetMonitoringRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_EnvironmentalAssetMonitoring_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT EnvironmentalAssetMonitoringId, TenantId, SiteId, Status, SensorId, MeasurementType, Value, ThresholdExceeded, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[EnvironmentalAssetMonitoring]
    WHERE IsDeleted = 0;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealthMonitoring_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single EnvironmentalHealthMonitoring row by identifier, backing
--                  IEnvironmentalHealthMonitoringRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EnvironmentalHealthMonitoring_GetById]
(
    @EnvironmentalHealthMonitoringId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EnvironmentalHealthMonitoringId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [MonitoringLocation], [MonitoredByUserId], [MeasuredAtUtc], [NextMeasurementDueAtUtc], [ExposureLevelScore], [ExceedanceThreshold], [ExceedanceCount], [CorrectiveActionDescription], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[EnvironmentalHealthMonitoring]
    WHERE [EnvironmentalHealthMonitoringId] = @EnvironmentalHealthMonitoringId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

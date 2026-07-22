-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealthMonitoring_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EnvironmentalHealthMonitoring rows, backing
--                  IEnvironmentalHealthMonitoringRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EnvironmentalHealthMonitoring_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EnvironmentalHealthMonitoringId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [MonitoringLocation], [MonitoredByUserId], [MeasuredAtUtc], [NextMeasurementDueAtUtc], [ExposureLevelScore], [ExceedanceThreshold], [ExceedanceCount], [CorrectiveActionDescription], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[EnvironmentalHealthMonitoring]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

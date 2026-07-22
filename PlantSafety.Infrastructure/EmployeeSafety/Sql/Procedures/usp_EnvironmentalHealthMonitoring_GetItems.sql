-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealthMonitoring_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IEnvironmentalHealthMonitoringRepository.GetEnvironmentalHealthMonitoringItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EnvironmentalHealthMonitoring_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [EnvironmentalHealthMonitoringId],
        [TenantId],
        [SiteId],
        [MonitoringTitle],
        [Category] AS [MonitoringCategory],
        [MonitoringLocation],
        [Status] AS [MonitoringStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [EmployeeSafety].[EnvironmentalHealthMonitoring]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

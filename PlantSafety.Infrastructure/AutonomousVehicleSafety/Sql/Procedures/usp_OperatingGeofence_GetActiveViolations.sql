-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperatingGeofence_GetActiveViolations.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IOperatingGeofenceRepository.GetActiveGeofenceViolationsAsync (Application
--                  read-model, ListActiveGeofenceViolations query). Returns Active geofences with
--                  at least one recorded violation, created within the requested UTC date range.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_GetActiveViolations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [OperatingGeofenceId],
        [TenantId],
        [SiteId],
        [GeofenceName],
        [ZoneType],
        [ViolationCount],
        [Status] AS [Status],
        [LastAuditedAtUtc],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [AutonomousVehicleSafety].[OperatingGeofence]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] = 'Active'
      AND [ViolationCount] > 0
      AND ISNULL([IsDeleted], 0) = 0
      AND [CreatedAtUtc] >= @FromUtc
      AND [CreatedAtUtc] <= @ToUtc
    ORDER BY [ViolationCount] DESC, [CreatedAtUtc] DESC;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperatingGeofence_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single OperatingGeofence row by identifier, backing
--                  IOperatingGeofenceRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_GetById]
(
    @OperatingGeofenceId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [OperatingGeofenceId], [TenantId], [SiteId], [Status], [GeofenceName], [ZoneType], [BoundaryDefinition], [MaxSpeedKph], [ViolationCount], [LastAuditedAtUtc], [NextAuditDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[OperatingGeofence]
    WHERE [OperatingGeofenceId] = @OperatingGeofenceId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

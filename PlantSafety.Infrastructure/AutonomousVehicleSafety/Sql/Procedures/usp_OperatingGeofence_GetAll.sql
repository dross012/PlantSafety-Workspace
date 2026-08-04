-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperatingGeofence_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted OperatingGeofence rows, backing
--                  IOperatingGeofenceRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [OperatingGeofenceId], [TenantId], [SiteId], [Status], [GeofenceName], [ZoneType], [BoundaryDefinition], [MaxSpeedKph], [ViolationCount], [LastAuditedAtUtc], [NextAuditDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[OperatingGeofence]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

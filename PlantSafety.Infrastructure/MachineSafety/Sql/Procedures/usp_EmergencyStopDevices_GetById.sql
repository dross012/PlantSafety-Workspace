-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.EmergencyStopDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyStopDevices_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single EmergencyStopDevices row by identifier, backing
--                  IEmergencyStopDevicesRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_EmergencyStopDevices_GetById]
(
    @EmergencyStopDevicesId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyStopDevicesId], [TenantId], [SiteId], [Status], [DeviceTitle], [DeviceType], [EquipmentLocation], [TestedByUserId], [TestedAtUtc], [NextTestDueAtUtc], [FailureCount], [ResponseTimeScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[EmergencyStopDevices]
    WHERE [EmergencyStopDevicesId] = @EmergencyStopDevicesId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.EmergencyStopDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyStopDevices_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EmergencyStopDevices rows, backing
--                  IEmergencyStopDevicesRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_EmergencyStopDevices_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyStopDevicesId], [TenantId], [SiteId], [Status], [DeviceTitle], [DeviceType], [EquipmentLocation], [TestedByUserId], [TestedAtUtc], [NextTestDueAtUtc], [FailureCount], [ResponseTimeScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[EmergencyStopDevices]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

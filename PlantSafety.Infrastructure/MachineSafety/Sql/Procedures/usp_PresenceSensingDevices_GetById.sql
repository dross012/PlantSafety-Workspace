-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.PresenceSensingDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PresenceSensingDevices_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single PresenceSensingDevices row by identifier, backing
--                  IPresenceSensingDevicesRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_PresenceSensingDevices_GetById]
(
    @PresenceSensingDevicesId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PresenceSensingDevicesId], [TenantId], [SiteId], [Status], [DeviceTitle], [DeviceType], [EquipmentLocation], [CalibratedByUserId], [CalibratedAtUtc], [NextCalibrationDueAtUtc], [TriggerEventCount], [DetectionAccuracyScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[PresenceSensingDevices]
    WHERE [PresenceSensingDevicesId] = @PresenceSensingDevicesId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

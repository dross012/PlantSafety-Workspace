-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.PresenceSensingDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PresenceSensingDevices_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PresenceSensingDevices rows, backing
--                  IPresenceSensingDevicesRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_PresenceSensingDevices_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PresenceSensingDevicesId], [TenantId], [SiteId], [Status], [DeviceTitle], [DeviceType], [EquipmentLocation], [CalibratedByUserId], [CalibratedAtUtc], [NextCalibrationDueAtUtc], [TriggerEventCount], [DetectionAccuracyScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[PresenceSensingDevices]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

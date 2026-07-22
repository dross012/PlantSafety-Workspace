-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.EmergencyStopDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyStopDevices_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IEmergencyStopDevicesRepository.GetEmergencyStopDevicesItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_EmergencyStopDevices_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [EmergencyStopDevicesId],
        [TenantId],
        [SiteId],
        [DeviceTitle],
        [DeviceType] AS [DeviceType],
        [EquipmentLocation],
        [Status] AS [DeviceStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [MachineSafety].[EmergencyStopDevices]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

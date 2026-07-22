-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.PresenceSensingDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PresenceSensingDevices_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [MachineSafety].[PresenceSensingDevices], backing
--                  IPresenceSensingDevicesRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_PresenceSensingDevices_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @PresenceSensingDevicesId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @DeviceTitle NVARCHAR(400),
    @DeviceType NVARCHAR(100),
    @EquipmentLocation NVARCHAR(400),
    @CalibratedByUserId UNIQUEIDENTIFIER,
    @CalibratedAtUtc DATETIME2(3) = NULL,
    @NextCalibrationDueAtUtc DATETIME2(3) = NULL,
    @TriggerEventCount INT,
    @DetectionAccuracyScore DECIMAL(9,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [MachineSafety].[PresenceSensingDevices]
            ([PresenceSensingDevicesId], [TenantId], [SiteId], [Status], [DeviceTitle], [DeviceType], [EquipmentLocation], [CalibratedByUserId], [CalibratedAtUtc], [NextCalibrationDueAtUtc], [TriggerEventCount], [DetectionAccuracyScore], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@PresenceSensingDevicesId, @TenantId, @SiteId, @Status, @DeviceTitle, @DeviceType, @EquipmentLocation, @CalibratedByUserId, @CalibratedAtUtc, @NextCalibrationDueAtUtc, @TriggerEventCount, @DetectionAccuracyScore, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

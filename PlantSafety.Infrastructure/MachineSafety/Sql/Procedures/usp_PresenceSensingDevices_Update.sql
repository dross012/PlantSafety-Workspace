-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.PresenceSensingDevices
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PresenceSensingDevices_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [MachineSafety].[PresenceSensingDevices], backing
--                  IPresenceSensingDevicesRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_PresenceSensingDevices_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
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
        UPDATE [MachineSafety].[PresenceSensingDevices]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [DeviceTitle] = @DeviceTitle,
        [DeviceType] = @DeviceType,
        [EquipmentLocation] = @EquipmentLocation,
        [CalibratedByUserId] = @CalibratedByUserId,
        [CalibratedAtUtc] = @CalibratedAtUtc,
        [NextCalibrationDueAtUtc] = @NextCalibrationDueAtUtc,
        [TriggerEventCount] = @TriggerEventCount,
        [DetectionAccuracyScore] = @DetectionAccuracyScore,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [PresenceSensingDevicesId] = @PresenceSensingDevicesId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

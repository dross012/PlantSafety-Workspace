-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleAsset_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [AutonomousVehicleSafety].[AutonomousVehicleAsset],
--                  backing IAutonomousVehicleAssetRepository.UpdateAsync (all lifecycle transitions
--                  persist the aggregate's full current state). Standard error envelope: TRY/CATCH
--                  with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AssetTag NVARCHAR(200),
    @VehicleType NVARCHAR(100),
    @AutonomyLevel NVARCHAR(100),
    @CurrentAutonomyMode NVARCHAR(100),
    @SafetyCaseValidationId UNIQUEIDENTIFIER = NULL,
    @ModeTransitionCount INT,
    @SuspensionReason NVARCHAR(1000) = NULL,
    @SuspendedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [AutonomousVehicleSafety].[AutonomousVehicleAsset]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AssetTag] = @AssetTag,
        [VehicleType] = @VehicleType,
        [AutonomyLevel] = @AutonomyLevel,
        [CurrentAutonomyMode] = @CurrentAutonomyMode,
        [SafetyCaseValidationId] = @SafetyCaseValidationId,
        [ModeTransitionCount] = @ModeTransitionCount,
        [SuspensionReason] = @SuspensionReason,
        [SuspendedAtUtc] = @SuspendedAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

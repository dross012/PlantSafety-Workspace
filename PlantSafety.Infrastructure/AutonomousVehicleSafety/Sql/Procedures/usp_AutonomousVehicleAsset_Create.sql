-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleAsset_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [AutonomousVehicleSafety].[AutonomousVehicleAsset],
--                  backing IAutonomousVehicleAssetRepository.AddAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @SuspendedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [AutonomousVehicleSafety].[AutonomousVehicleAsset]
            ([AutonomousVehicleAssetId], [TenantId], [SiteId], [Status], [AssetTag], [VehicleType], [AutonomyLevel], [CurrentAutonomyMode], [SafetyCaseValidationId], [ModeTransitionCount], [SuspensionReason], [SuspendedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@AutonomousVehicleAssetId, @TenantId, @SiteId, @Status, @AssetTag, @VehicleType, @AutonomyLevel, @CurrentAutonomyMode, @SafetyCaseValidationId, @ModeTransitionCount, @SuspensionReason, @SuspendedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

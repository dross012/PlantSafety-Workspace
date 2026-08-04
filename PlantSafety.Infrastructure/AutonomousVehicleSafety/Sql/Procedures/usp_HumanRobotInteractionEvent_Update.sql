-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HumanRobotInteractionEvent_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for
--                  [AutonomousVehicleSafety].[HumanRobotInteractionEvent], backing
--                  IHumanRobotInteractionEventRepository.UpdateAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @HumanRobotInteractionEventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
    @InteractionType NVARCHAR(100),
    @SeverityLevel NVARCHAR(100),
    @DistanceMeters DECIMAL(9,3) = NULL,
    @LocationDescription NVARCHAR(400),
    @DetectedAtUtc DATETIME2(3),
    @ReviewedByUserId UNIQUEIDENTIFIER = NULL,
    @ReviewedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId,
        [InteractionType] = @InteractionType,
        [SeverityLevel] = @SeverityLevel,
        [DistanceMeters] = @DistanceMeters,
        [LocationDescription] = @LocationDescription,
        [DetectedAtUtc] = @DetectedAtUtc,
        [ReviewedByUserId] = @ReviewedByUserId,
        [ReviewedAtUtc] = @ReviewedAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [HumanRobotInteractionEventId] = @HumanRobotInteractionEventId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

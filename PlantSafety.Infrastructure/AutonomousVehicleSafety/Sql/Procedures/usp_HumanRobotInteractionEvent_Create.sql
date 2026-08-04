-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HumanRobotInteractionEvent_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [AutonomousVehicleSafety].[HumanRobotInteractionEvent],
--                  backing IHumanRobotInteractionEventRepository.AddAsync. Standard error
--                  envelope: TRY/CATCH with transaction rollback and THROW. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @ReviewedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
            ([HumanRobotInteractionEventId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [InteractionType], [SeverityLevel], [DistanceMeters], [LocationDescription], [DetectedAtUtc], [ReviewedByUserId], [ReviewedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@HumanRobotInteractionEventId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @InteractionType, @SeverityLevel, @DistanceMeters, @LocationDescription, @DetectedAtUtc, @ReviewedByUserId, @ReviewedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

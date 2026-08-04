-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HumanRobotInteractionEvent_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single HumanRobotInteractionEvent row by identifier, backing
--                  IHumanRobotInteractionEventRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_GetById]
(
    @HumanRobotInteractionEventId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HumanRobotInteractionEventId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [InteractionType], [SeverityLevel], [DistanceMeters], [LocationDescription], [DetectedAtUtc], [ReviewedByUserId], [ReviewedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
    WHERE [HumanRobotInteractionEventId] = @HumanRobotInteractionEventId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

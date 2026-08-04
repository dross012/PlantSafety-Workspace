-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HumanRobotInteractionEvent_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IHumanRobotInteractionEventRepository.GetHumanRobotInteractionEventItemsAsync
--                  (Application read-model, ListHumanRobotInteractionEvents query).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_HumanRobotInteractionEvent_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [HumanRobotInteractionEventId],
        [TenantId],
        [SiteId],
        [AutonomousVehicleAssetId],
        [InteractionType],
        [SeverityLevel],
        [DistanceMeters],
        [Status] AS [Status],
        [DetectedAtUtc],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [AutonomousVehicleSafety].[HumanRobotInteractionEvent]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND [DetectedAtUtc] >= @FromUtc
      AND [DetectedAtUtc] <= @ToUtc
    ORDER BY [DetectedAtUtc] DESC;
END
GO

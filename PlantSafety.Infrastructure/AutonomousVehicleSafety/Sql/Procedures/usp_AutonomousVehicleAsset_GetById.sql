-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleAsset_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single AutonomousVehicleAsset row by identifier, backing
--                  IAutonomousVehicleAssetRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_GetById]
(
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AutonomousVehicleAssetId], [TenantId], [SiteId], [Status], [AssetTag], [VehicleType], [AutonomyLevel], [CurrentAutonomyMode], [SafetyCaseValidationId], [ModeTransitionCount], [SuspensionReason], [SuspendedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[AutonomousVehicleAsset]
    WHERE [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

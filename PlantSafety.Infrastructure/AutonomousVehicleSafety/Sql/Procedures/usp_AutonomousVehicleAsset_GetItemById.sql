-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleAsset_GetItemById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAutonomousVehicleAssetRepository.GetAutonomousVehicleAssetByIdAsync
--                  (Application read-model, GetAutonomousVehicleAssetById query).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_GetItemById]
    @TenantId UNIQUEIDENTIFIER,
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AutonomousVehicleAssetId],
        [TenantId],
        [SiteId],
        [AssetTag],
        [VehicleType],
        [AutonomyLevel],
        [CurrentAutonomyMode],
        [ModeTransitionCount],
        [Status] AS [Status],
        [SuspensionReason],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [AutonomousVehicleSafety].[AutonomousVehicleAsset]
    WHERE [TenantId] = @TenantId
      AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleAsset_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted AutonomousVehicleAsset rows, backing
--                  IAutonomousVehicleAssetRepository.ListAsync (the Domain specification predicate
--                  carries no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleAsset_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AutonomousVehicleAssetId], [TenantId], [SiteId], [Status], [AssetTag], [VehicleType], [AutonomyLevel], [CurrentAutonomyMode], [SafetyCaseValidationId], [ModeTransitionCount], [SuspensionReason], [SuspendedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[AutonomousVehicleAsset]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

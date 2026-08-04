-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleIncident_GetHistoryByAssetId.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IAutonomousVehicleIncidentRepository.GetIncidentHistoryByAssetIdAsync
--                  (Application read-model, GetIncidentHistoryByAsset query).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_GetHistoryByAssetId]
    @TenantId UNIQUEIDENTIFIER,
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AutonomousVehicleIncidentId],
        [TenantId],
        [SiteId],
        [AutonomousVehicleAssetId],
        [IncidentType],
        [SeverityLevel],
        [Status] AS [Status],
        [SuspensionTriggered],
        [OccurredAtUtc],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [AutonomousVehicleSafety].[AutonomousVehicleIncident]
    WHERE [TenantId] = @TenantId
      AND [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [OccurredAtUtc] DESC;
END
GO

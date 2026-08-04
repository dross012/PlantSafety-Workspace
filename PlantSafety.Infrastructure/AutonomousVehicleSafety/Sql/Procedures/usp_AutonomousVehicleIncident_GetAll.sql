-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AutonomousVehicleIncident_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted AutonomousVehicleIncident rows, backing
--                  IAutonomousVehicleIncidentRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_AutonomousVehicleIncident_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AutonomousVehicleIncidentId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [IncidentType], [SeverityLevel], [OccurredAtUtc], [DescriptionSummary], [InvestigationLeadUserId], [SuspensionTriggered], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [AutonomousVehicleSafety].[AutonomousVehicleIncident]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

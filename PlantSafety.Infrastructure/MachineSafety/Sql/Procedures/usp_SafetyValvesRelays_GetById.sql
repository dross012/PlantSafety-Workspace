-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.SafetyValvesRelays
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyValvesRelays_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single SafetyValvesRelays row by identifier, backing
--                  ISafetyValvesRelaysRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_SafetyValvesRelays_GetById]
(
    @SafetyValvesRelaysId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SafetyValvesRelaysId], [TenantId], [SiteId], [Status], [ComponentTitle], [ComponentType], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [FaultCount], [ReliabilityScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[SafetyValvesRelays]
    WHERE [SafetyValvesRelaysId] = @SafetyValvesRelaysId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

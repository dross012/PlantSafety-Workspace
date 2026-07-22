-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.SafetyValvesRelays
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyValvesRelays_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted SafetyValvesRelays rows, backing
--                  ISafetyValvesRelaysRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_SafetyValvesRelays_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SafetyValvesRelaysId], [TenantId], [SiteId], [Status], [ComponentTitle], [ComponentType], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [FaultCount], [ReliabilityScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[SafetyValvesRelays]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

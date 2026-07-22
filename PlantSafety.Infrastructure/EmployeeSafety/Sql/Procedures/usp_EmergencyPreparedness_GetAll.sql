-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyPreparedness_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EmergencyPreparedness rows, backing
--                  IEmergencyPreparednessRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EmergencyPreparedness_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyPreparednessId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [CoordinatedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [ReadinessThreshold], [ReadinessGapNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[EmergencyPreparedness]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

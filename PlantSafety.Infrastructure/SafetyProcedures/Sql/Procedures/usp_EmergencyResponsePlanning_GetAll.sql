-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyResponsePlanning_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EmergencyResponsePlanning rows, backing
--                  IEmergencyResponsePlanningRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_EmergencyResponsePlanning_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyResponsePlanningId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [ApprovedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[EmergencyResponsePlanning]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

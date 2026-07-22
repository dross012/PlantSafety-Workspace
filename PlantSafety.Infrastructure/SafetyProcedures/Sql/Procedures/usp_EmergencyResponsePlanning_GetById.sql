-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyResponsePlanning_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single EmergencyResponsePlanning row by identifier, backing
--                  IEmergencyResponsePlanningRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_EmergencyResponsePlanning_GetById]
(
    @EmergencyResponsePlanningId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyResponsePlanningId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [ApprovedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[EmergencyResponsePlanning]
    WHERE [EmergencyResponsePlanningId] = @EmergencyResponsePlanningId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

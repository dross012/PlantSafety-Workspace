-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyPreparedness_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single EmergencyPreparedness row by identifier, backing
--                  IEmergencyPreparednessRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EmergencyPreparedness_GetById]
(
    @EmergencyPreparednessId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EmergencyPreparednessId], [TenantId], [SiteId], [Status], [PlanTitle], [EmergencyType], [EvacuationRoute], [CoordinatedByUserId], [DrillConductedAtUtc], [NextDrillDueAtUtc], [ParticipantCount], [ReadinessScore], [ReadinessThreshold], [ReadinessGapNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[EmergencyPreparedness]
    WHERE [EmergencyPreparednessId] = @EmergencyPreparednessId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

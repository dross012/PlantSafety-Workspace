-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_JointResponsePlans_GetByIncident.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ILocalEmergencyCoordinationRepository.GetJointResponsePlansByIncidentAsync
--                  (Application read-model) -- the Active Joint Response Status dashboard.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_JointResponsePlans_GetByIncident]
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT JointResponsePlanId, LocalEmergencyCoordinationId, TenantId, IncidentId, PlanName, IsActive, ActivatedAtUtc, ActivatedBy, DeactivatedAtUtc
    FROM [SafetyEvents].[JointResponsePlans]
    WHERE TenantId = @TenantId AND IncidentId = @IncidentId
    ORDER BY ActivatedAtUtc DESC;
END
GO

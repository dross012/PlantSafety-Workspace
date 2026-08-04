-- ============================================================================
--  Module:         CommunitySafety.CommunityImpactAssessment
--  File:           usp_CommunityImpactAssessment_GetByIncident.sql
--  Created:        2026-08-04
--  Description:    Read-only stored procedure backing
--                  ICommunityImpactAssessmentRepository.GetImpactAssessmentItemsByIncidentAsync
--                  (Application read-model for GetImpactAssessmentsByIncident query / Impact
--                  Assessment Summary by incident dashboard). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_GetByIncident]
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CommunityImpactAssessmentId], [TenantId], [SiteId], [IncidentId], [Severity], [AffectedArea],
        [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting],
        [Status] AS [AssessmentStatus], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityImpactAssessment]
    WHERE [TenantId] = @TenantId
      AND [IncidentId] = @IncidentId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [AssessedAtUtc] DESC;
END
GO

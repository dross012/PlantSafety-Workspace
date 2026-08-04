-- ============================================================================
--  Module:         CommunitySafety.CommunityImpactAssessment
--  File:           usp_CommunityImpactAssessment_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single CommunityImpactAssessment row by identifier, backing
--                  ICommunityImpactAssessmentRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_GetById]
(
    @CommunityImpactAssessmentId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CommunityImpactAssessmentId], [TenantId], [SiteId], [Status], [IncidentId], [Severity], [AffectedArea], [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting], [AssessedByUserId], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy], [FinalizedAtUtc]
    FROM [CommunitySafety].[CommunityImpactAssessment]
    WHERE [CommunityImpactAssessmentId] = @CommunityImpactAssessmentId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CultureAssessment_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing ICultureAssessmentRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_CultureAssessment_GetById]
    @CultureAssessmentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [CultureAssessmentId], [TenantId], [SiteId], [Status], [AssessmentTitle],
    [PeriodStartUtc], [PeriodEndUtc], [OverallCultureScore], [LeadershipEngagementScore],
    [Summary], [RelatedRiskManagementHazardId], [CreatedAtUtc], [CreatedBy], [PublishedAtUtc]
FROM [SafetyCulture].[CultureAssessment]
WHERE [CultureAssessmentId] = @CultureAssessmentId
  AND ISNULL([IsDeleted], 0) = 0;
GO

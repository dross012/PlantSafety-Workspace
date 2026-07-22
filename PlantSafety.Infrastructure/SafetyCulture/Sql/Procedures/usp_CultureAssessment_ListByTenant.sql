-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CultureAssessment_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing ICultureAssessmentRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; CultureAssessmentSpecification
--                  predicates are applied in-memory in the repository after rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_CultureAssessment_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [CultureAssessmentId], [TenantId], [SiteId], [Status], [AssessmentTitle],
    [PeriodStartUtc], [PeriodEndUtc], [OverallCultureScore], [LeadershipEngagementScore],
    [Summary], [RelatedRiskManagementHazardId], [CreatedAtUtc], [CreatedBy], [PublishedAtUtc]
FROM [SafetyCulture].[CultureAssessment]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC;
GO

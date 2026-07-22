-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.HazardAssessments
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardAssessments_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[HazardAssessmentsReadModel],
--                  backing IHazardAssessmentsRepository.GetHazardAssessmentsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_HazardAssessments_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [HazardAssessmentId],
        [TenantId],
        [SiteId],
        [HazardDescription],
        [RiskLevel],
        [Status],
        [AssessedAtUtc],
        [AssessedBy]
FROM [Operational].[HazardAssessmentsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
GO

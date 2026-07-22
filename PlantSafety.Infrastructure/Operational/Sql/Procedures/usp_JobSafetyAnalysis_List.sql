-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.JobSafetyAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_JobSafetyAnalysis_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[JobSafetyAnalysisReadModel],
--                  backing IJobSafetyAnalysisRepository.GetJobSafetyAnalysisAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_JobSafetyAnalysis_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [JobSafetyAnalysisId],
        [TenantId],
        [SiteId],
        [TaskName],
        [RiskLevel],
        [Status],
        [AnalyzedAtUtc],
        [AnalyzedBy]
FROM [Operational].[JobSafetyAnalysisReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AnalyzedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AnalyzedAtUtc] DESC;
GO

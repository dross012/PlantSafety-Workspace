-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.EngagementScoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EngagementScoring_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[EngagementScoring],
--                  backing IEngagementScoringRepository.GetEngagementScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementScoring_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [EngagementScore],
        [ScoreCategory],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[EngagementScoring]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
GO

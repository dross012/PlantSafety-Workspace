-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Training
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ScoreCardsTraining_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[ScoreCardsTraining],
--                  backing ITrainingRepository.GetTrainingScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsTraining_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ScoreCardId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [CompletionRate],
        [CoursesCompleted],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[ScoreCardsTraining]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
GO

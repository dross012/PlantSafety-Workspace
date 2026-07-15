-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Compliance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ScoreCardsCompliance_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[ScoreCardsCompliance],
--                  backing IComplianceRepository.GetComplianceScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsCompliance_List]
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
        [ComplianceArea],
        [ScoreValue],
        [MaxScore],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[ScoreCardsCompliance]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
GO

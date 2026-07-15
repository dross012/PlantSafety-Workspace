-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Supervisor
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Supervisor_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[Supervisor],
--                  backing ISupervisorRepository.GetSupervisorScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Supervisor_List]
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
        [SupervisorId],
        [ScoreValue],
        [TeamSize],
        [Status],
        [ScoredAtUtc]
FROM [Analytics].[Supervisor]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScoredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScoredAtUtc] DESC;
GO

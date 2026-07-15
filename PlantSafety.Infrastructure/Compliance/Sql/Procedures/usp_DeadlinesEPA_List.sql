-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.DeadlinesEPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DeadlinesEPA_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[DeadlinesEPA],
--                  backing IEPARepository.GetDeadlinesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_DeadlinesEPA_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DeadlineId],
        [TenantId],
        [SiteId],
        [EPARegulation],
        [DueAtUtc],
        [Status],
        [ResponsibleParty]
FROM [Compliance].[DeadlinesEPA]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [DueAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DueAtUtc] DESC;
GO
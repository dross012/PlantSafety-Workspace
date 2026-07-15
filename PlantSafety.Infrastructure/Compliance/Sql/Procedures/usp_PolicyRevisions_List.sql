-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyRevisions
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PolicyRevisions_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[PolicyRevisions],
--                  backing IRevisionsRepository.GetRevisionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyRevisions_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RevisionId],
        [TenantId],
        [SiteId],
        [PolicyId],
        [RevisionNumber],
        [Status],
        [RevisedAtUtc],
        [RevisedBy]
FROM [Compliance].[PolicyRevisions]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RevisedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RevisedAtUtc] DESC;
GO
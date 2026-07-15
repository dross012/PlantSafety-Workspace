-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyDocuments
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PolicyDocuments_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[PolicyDocuments],
--                  backing IDocumentsRepository.GetDocumentsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyDocuments_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [DocumentId],
        [TenantId],
        [SiteId],
        [Title],
        [DocumentType],
        [Status],
        [PublishedAtUtc],
        [OwnerId]
FROM [Compliance].[PolicyDocuments]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PublishedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PublishedAtUtc] DESC;
GO
-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Process_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Deterministic paginated list procedure for [ProcessManagement].[Processes],
--                  backing IProcessRepository.ListAsync. Explicit ORDER BY + OFFSET/FETCH paging;
--                  windowed TotalCount column avoids a second round trip.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_Process_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @Category       NVARCHAR(50)     = NULL,
    @Status         NVARCHAR(50)     = NULL,
    @OrderBy        NVARCHAR(100)    = 'ProcessName',
    @SortDirection  NVARCHAR(4)      = 'ASC',
    @PageNumber     INT              = 1,
    @PageSize       INT              = 25
AS
SET NOCOUNT ON;

SELECT
        [ProcessId],
        [TenantId],
        [SiteId],
        [ProcessName],
        [Category],
        [Status],
        [Version],
        [OwnerId],
        [LastModifiedAtUtc],
        CONVERT(NVARCHAR(34), CAST([RowVersion] AS BINARY(8)), 1) AS [ETag],
        COUNT(*) OVER() AS [TotalCount]
FROM [ProcessManagement].[Processes]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@Category IS NULL OR [Category] = @Category)
  AND (@Status IS NULL OR [Status] = @Status)
ORDER BY
    CASE WHEN @OrderBy = 'ProcessName' AND @SortDirection = 'ASC'  THEN [ProcessName] END ASC,
    CASE WHEN @OrderBy = 'ProcessName' AND @SortDirection = 'DESC' THEN [ProcessName] END DESC,
    CASE WHEN @OrderBy = 'Category'    AND @SortDirection = 'ASC'  THEN [Category] END ASC,
    CASE WHEN @OrderBy = 'Category'    AND @SortDirection = 'DESC' THEN [Category] END DESC,
    CASE WHEN @OrderBy = 'Status'      AND @SortDirection = 'ASC'  THEN [Status] END ASC,
    CASE WHEN @OrderBy = 'Status'      AND @SortDirection = 'DESC' THEN [Status] END DESC,
    [ProcessName] ASC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
GO

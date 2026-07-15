-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_WorkerWellBeing_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Deterministic paginated list procedure for
--                  [WorkerWellBeing].[WorkerWellBeingRecords], backing
--                  IWorkerWellBeingRecordRepository.ListAsync. Explicit ORDER BY + OFFSET/FETCH
--                  paging; windowed TotalCount column avoids a second round trip.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [WorkerWellBeing].[usp_WorkerWellBeing_List]
    @TenantId           UNIQUEIDENTIFIER,
    @WorkerId            UNIQUEIDENTIFIER = NULL,
    @WellBeingRiskLevel  NVARCHAR(20)     = NULL,
    @Status              NVARCHAR(50)     = NULL,
    @OrderBy             NVARCHAR(100)    = 'RecordedAtUtc',
    @SortDirection       NVARCHAR(4)      = 'DESC',
    @PageNumber          INT              = 1,
    @PageSize            INT              = 25
AS
SET NOCOUNT ON;

SELECT
        [WorkerWellBeingId],
        [TenantId],
        [WorkerId],
        [WellBeingRiskLevel],
        [Status],
        [RecordedAtUtc],
        COUNT(*) OVER() AS [TotalCount]
FROM [WorkerWellBeing].[WorkerWellBeingRecords]
WHERE [TenantId] = @TenantId
  AND (@WorkerId IS NULL OR [WorkerId] = @WorkerId)
  AND (@WellBeingRiskLevel IS NULL OR [WellBeingRiskLevel] = @WellBeingRiskLevel)
  AND (@Status IS NULL OR [Status] = @Status)
ORDER BY
    CASE WHEN @OrderBy = 'RecordedAtUtc'      AND @SortDirection = 'ASC'  THEN [RecordedAtUtc] END ASC,
    CASE WHEN @OrderBy = 'RecordedAtUtc'      AND @SortDirection = 'DESC' THEN [RecordedAtUtc] END DESC,
    CASE WHEN @OrderBy = 'WellBeingRiskLevel' AND @SortDirection = 'ASC'  THEN [WellBeingRiskLevel] END ASC,
    CASE WHEN @OrderBy = 'WellBeingRiskLevel' AND @SortDirection = 'DESC' THEN [WellBeingRiskLevel] END DESC,
    CASE WHEN @OrderBy = 'Status'             AND @SortDirection = 'ASC'  THEN [Status] END ASC,
    CASE WHEN @OrderBy = 'Status'             AND @SortDirection = 'DESC' THEN [Status] END DESC,
    [RecordedAtUtc] DESC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;
GO

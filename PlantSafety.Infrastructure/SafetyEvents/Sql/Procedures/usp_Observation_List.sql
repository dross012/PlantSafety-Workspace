-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Observation_List.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [SafetyEvents].[Observation] rows.
--                  Backs IObservationRepository.ListAsync / ObservationDapperRepository.ListAsync.
--                  Parameter names/types mirror the StoredProcedureBuilder Add* calls in
--                  ObservationDapperRepository.ListAsync's listParameters exactly.
--                  ObservationDapperRepository.ListAsync issues a SEPARATE usp_Observation_Count
--                  call for the total (not a windowed COUNT(*) OVER()), so this proc does not
--                  project a TotalCount column, matching that call shape exactly. No caller
--                  currently invokes ListAsync (no List endpoint exists on ObservationsV1Controller
--                  today), built here for interface completeness. Deterministic ORDER BY +
--                  OFFSET/FETCH paging with a fixed ObservationId tiebreaker. TRY/CATCH + THROW
--                  only (no AuditLog/ErrorLog writes) per the established read-procedure
--                  convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Observation_List]
(
    @TenantId        UNIQUEIDENTIFIER,
    @SiteId          UNIQUEIDENTIFIER = NULL,
    @ObservationType NVARCHAR(30)     = NULL,
    @FromUtc         DATETIME2(3)     = NULL,
    @ToUtc           DATETIME2(3)     = NULL,
    @OrderBy         NVARCHAR(64)     = 'ObservedAtUtc',
    @SortDirection   NVARCHAR(4)      = 'DESC',
    @PageNumber      INT              = 1,
    @PageSize        INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            ObservationId,
            TenantId,
            SiteId,
            Title,
            Description,
            ObservationType,
            ObservedAtUtc,
            ObservedByWorkerId,
            Status,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[Observation]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@ObservationType IS NULL OR ObservationType = @ObservationType)
          AND (@FromUtc IS NULL OR ObservedAtUtc >= @FromUtc)
          AND (@ToUtc IS NULL OR ObservedAtUtc <= @ToUtc)
        ORDER BY
            CASE WHEN @OrderBy = 'ObservedAtUtc' AND @SortDirection = 'ASC'  THEN ObservedAtUtc END ASC,
            CASE WHEN @OrderBy = 'ObservedAtUtc' AND @SortDirection = 'DESC' THEN ObservedAtUtc END DESC,
            CASE WHEN @OrderBy = 'Title'         AND @SortDirection = 'ASC'  THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title'         AND @SortDirection = 'DESC' THEN Title END DESC,
            CASE WHEN @OrderBy = 'ObservationType' AND @SortDirection = 'ASC'  THEN ObservationType END ASC,
            CASE WHEN @OrderBy = 'ObservationType' AND @SortDirection = 'DESC' THEN ObservationType END DESC,
            CASE WHEN @OrderBy = 'ObservationId' AND @SortDirection = 'DESC' THEN ObservationId END DESC,
            ObservedAtUtc DESC, ObservationId ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

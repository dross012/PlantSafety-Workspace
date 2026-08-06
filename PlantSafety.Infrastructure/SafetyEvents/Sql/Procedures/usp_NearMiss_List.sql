-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMiss_List.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [SafetyEvents].[NearMiss] rows. Backs
--                  INearMissRepository.ListAsync / NearMissDapperRepository.ListAsync. Parameter
--                  names/types mirror the StoredProcedureBuilder Add* calls in
--                  NearMissDapperRepository.ListAsync's listParameters exactly (does not include
--                  TotalCount -- ListNearMissesQuery's default OrderBy is 'OccurredAtUtc' DESC, so
--                  that is the default here too). NearMissDapperRepository.ListAsync issues a
--                  SEPARATE usp_NearMiss_Count call for the total, unlike the windowed-COUNT(*)
--                  OVER() pattern used elsewhere in this codebase -- this proc therefore does not
--                  project a TotalCount column, matching that call shape exactly. Deterministic
--                  ORDER BY + OFFSET/FETCH paging with a fixed NearMissId tiebreaker. TRY/CATCH +
--                  THROW only (no AuditLog/ErrorLog writes) per the established read-procedure
--                  convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMiss_List]
(
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @PotentialSeverity NVARCHAR(50)     = NULL,
    @HazardCategory    NVARCHAR(100)    = NULL,
    @OccurredFromUtc   DATETIME2(3)     = NULL,
    @OccurredToUtc     DATETIME2(3)     = NULL,
    @OrderBy           NVARCHAR(64)     = 'OccurredAtUtc',
    @SortDirection     NVARCHAR(4)      = 'DESC',
    @PageNumber        INT              = 1,
    @PageSize          INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            NearMissId,
            TenantId,
            SiteId,
            Title,
            Description,
            OccurredAtUtc,
            PotentialSeverity,
            HazardCategory,
            Status,
            ReportedByWorkerId,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[NearMiss]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@PotentialSeverity IS NULL OR PotentialSeverity = @PotentialSeverity)
          AND (@HazardCategory IS NULL OR HazardCategory = @HazardCategory)
          AND (@OccurredFromUtc IS NULL OR OccurredAtUtc >= @OccurredFromUtc)
          AND (@OccurredToUtc IS NULL OR OccurredAtUtc <= @OccurredToUtc)
        ORDER BY
            CASE WHEN @OrderBy = 'OccurredAtUtc' AND @SortDirection = 'ASC'  THEN OccurredAtUtc END ASC,
            CASE WHEN @OrderBy = 'OccurredAtUtc' AND @SortDirection = 'DESC' THEN OccurredAtUtc END DESC,
            CASE WHEN @OrderBy = 'Title'         AND @SortDirection = 'ASC'  THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title'         AND @SortDirection = 'DESC' THEN Title END DESC,
            CASE WHEN @OrderBy = 'PotentialSeverity' AND @SortDirection = 'ASC'  THEN PotentialSeverity END ASC,
            CASE WHEN @OrderBy = 'PotentialSeverity' AND @SortDirection = 'DESC' THEN PotentialSeverity END DESC,
            CASE WHEN @OrderBy = 'NearMissId'    AND @SortDirection = 'DESC' THEN NearMissId END DESC,
            OccurredAtUtc DESC, NearMissId ASC
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

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Drill_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of Drill records backing
--                  IDrillRepository.ListAsync. Deterministic ORDER BY + OFFSET/FETCH
--                  paging per Enterprise Architectural Rule Handbook §1.5/§2.10.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Drill_List]
(
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @Status        NVARCHAR(64)     = NULL,
    @DrillType     NVARCHAR(64)     = NULL,
    @OrderBy       NVARCHAR(64)     = 'ScheduledAtUtc',
    @SortDirection NVARCHAR(4)      = 'DESC',
    @PageNumber    INT              = 1,
    @PageSize      INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            DrillId,
            TenantId,
            SiteId,
            Title,
            Description,
            DrillType,
            ScheduledAtUtc,
            ExecutedAtUtc,
            OutcomeScore,
            Status,
            CoordinatorId,
            LastModifiedAtUtc,
            ETag
        FROM [SafetyEvents].[Drill]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@DrillType IS NULL OR DrillType = @DrillType)
        ORDER BY
            CASE WHEN @OrderBy = 'ScheduledAtUtc' AND @SortDirection = 'ASC' THEN ScheduledAtUtc END ASC,
            CASE WHEN @OrderBy = 'ScheduledAtUtc' AND @SortDirection = 'DESC' THEN ScheduledAtUtc END DESC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'ASC' THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'DESC' THEN Title END DESC,
            ScheduledAtUtc DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

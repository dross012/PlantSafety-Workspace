-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.CrisisManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CrisisEvent_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of CrisisEvent records backing
--                  ICrisisEventRepository.ListAsync. Deterministic ORDER BY + OFFSET/FETCH
--                  paging per Enterprise Architectural Rule Handbook §1.5/§2.10.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_CrisisEvent_List]
(
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @Status         NVARCHAR(64)     = NULL,
    @CrisisCategory NVARCHAR(64)     = NULL,
    @OrderBy        NVARCHAR(64)     = 'DeclaredAtUtc',
    @SortDirection  NVARCHAR(4)      = 'DESC',
    @PageNumber     INT              = 1,
    @PageSize       INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            CrisisEventId,
            TenantId,
            SiteId,
            Title,
            Description,
            CrisisCategory,
            DeclaredAtUtc,
            IncidentCommanderId,
            Status,
            ResolvedAtUtc,
            LastModifiedAtUtc,
            ETag
        FROM [SafetyEvents].[CrisisEvent]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@CrisisCategory IS NULL OR CrisisCategory = @CrisisCategory)
        ORDER BY
            CASE WHEN @OrderBy = 'DeclaredAtUtc' AND @SortDirection = 'ASC' THEN DeclaredAtUtc END ASC,
            CASE WHEN @OrderBy = 'DeclaredAtUtc' AND @SortDirection = 'DESC' THEN DeclaredAtUtc END DESC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'ASC' THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'DESC' THEN Title END DESC,
            DeclaredAtUtc DESC
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

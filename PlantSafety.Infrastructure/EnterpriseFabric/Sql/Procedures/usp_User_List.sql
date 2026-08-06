-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.Users
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_User_List.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [EnterpriseFabric].[User] rows.
--                  Backs IUserRepository.ListAsync / UserDapperRepository.ListAsync. Parameter
--                  names/types mirror the StoredProcedureBuilder Add* calls in
--                  UserDapperRepository.ListAsync exactly (TenantId, Role, SiteId, IsActive,
--                  OrderBy, SortDirection, PageNumber, PageSize). @Role/@SiteId filter against the
--                  delimited RolesCsv/SiteIdsCsv columns via a delimited-contains match (values are
--                  stored comma-joined, so an exact-token match needs the leading/trailing-comma
--                  wrap trick below rather than a plain LIKE '%x%', which would false-positive on
--                  substrings). Default OrderBy = 'UserId' matches ListUsersQuery's default.
--                  Windowed COUNT(*) OVER() AS TotalCount avoids a second round trip, matching the
--                  usp_CorrectiveAction_List precedent and UserDapperRepository.ListAsync's
--                  consumption pattern (rows[0].TotalCount). Deterministic ORDER BY + OFFSET/FETCH
--                  paging with a fixed UserId tiebreaker. TRY/CATCH + THROW only (no AuditLog/
--                  ErrorLog writes) per the established read-procedure convention. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_User_List]
(
    @TenantId      UNIQUEIDENTIFIER,
    @Role          NVARCHAR(100)  = NULL,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @IsActive      BIT            = NULL,
    @OrderBy       NVARCHAR(100)  = 'UserId',
    @SortDirection NVARCHAR(4)    = 'ASC',
    @PageNumber    INT            = 1,
    @PageSize      INT            = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            UserId,
            TenantId,
            Email,
            FirstName,
            LastName,
            RolesCsv,
            SiteIdsCsv,
            IsActive,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
            COUNT(*) OVER() AS TotalCount
        FROM [EnterpriseFabric].[User]
        WHERE TenantId = @TenantId
          AND (@Role IS NULL OR (',' + RolesCsv + ',') LIKE ('%,' + @Role + ',%'))
          AND (@SiteId IS NULL OR (',' + SiteIdsCsv + ',') LIKE ('%,' + CAST(@SiteId AS NVARCHAR(36)) + ',%'))
          AND (@IsActive IS NULL OR IsActive = @IsActive)
        ORDER BY
            CASE WHEN @OrderBy = 'Email'             AND @SortDirection = 'ASC'  THEN Email END ASC,
            CASE WHEN @OrderBy = 'Email'             AND @SortDirection = 'DESC' THEN Email END DESC,
            CASE WHEN @OrderBy = 'LastName'          AND @SortDirection = 'ASC'  THEN LastName END ASC,
            CASE WHEN @OrderBy = 'LastName'          AND @SortDirection = 'DESC' THEN LastName END DESC,
            CASE WHEN @OrderBy = 'LastModifiedAtUtc' AND @SortDirection = 'ASC'  THEN LastModifiedAtUtc END ASC,
            CASE WHEN @OrderBy = 'LastModifiedAtUtc' AND @SortDirection = 'DESC' THEN LastModifiedAtUtc END DESC,
            CASE WHEN @OrderBy = 'UserId'            AND @SortDirection = 'DESC' THEN UserId END DESC,
            UserId ASC
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

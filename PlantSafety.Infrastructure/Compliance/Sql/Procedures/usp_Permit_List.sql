-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Permit_List.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paged stored procedure backing IPermitRepository.ListAsync. Carries
--                  a window-function TotalCount column per row, matching
--                  PermitDapperRepository.ListAsync's PermitListRow shape.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Permit_List]
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @PermitType    NVARCHAR(100) = NULL,
    @Status        NVARCHAR(50) = NULL,
    @ValidFromUtc  DATETIME2(3) = NULL,
    @ValidToUtc    DATETIME2(3) = NULL,
    @OrderBy       NVARCHAR(100) = N'ValidFromUtc',
    @SortDirection NVARCHAR(4) = N'DESC',
    @PageNumber    INT = 1,
    @PageSize      INT = 25
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PermitId, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, WorkLocation, WorkLeadId,
        ValidFromUtc, ValidToUtc, Status, ApprovedByActorId, ApprovedAtUtc, LastModifiedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
        COUNT(1) OVER () AS TotalCount
    FROM [Compliance].[Permit]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@PermitType IS NULL OR PermitType = @PermitType)
      AND (@Status IS NULL OR Status = @Status)
      AND (@ValidFromUtc IS NULL OR ValidFromUtc >= @ValidFromUtc)
      AND (@ValidToUtc IS NULL OR ValidToUtc <= @ValidToUtc)
    ORDER BY
        CASE WHEN @OrderBy = N'ValidFromUtc' AND @SortDirection = N'ASC' THEN ValidFromUtc END ASC,
        CASE WHEN @OrderBy = N'ValidFromUtc' AND @SortDirection = N'DESC' THEN ValidFromUtc END DESC,
        ValidFromUtc DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

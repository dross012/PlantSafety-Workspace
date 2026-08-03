-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Reporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Reporting_GetListPage.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Deterministically ordered, paginated, optionally type-filtered page of the
--                  report catalog, backing IReportingRepository.GetReportsPagedAsync. Backs
--                  the PlantSafety.UI Reporting module's Reports List page.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Reporting_GetListPage]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @ReportType  NVARCHAR(100) = NULL,
    @PageNumber  INT = 1,
    @PageSize    INT = 25
AS
SET NOCOUNT ON;

SELECT
        [ReportItemId],
        [ReportName],
        [ReportCategory],
        [Status],
        [GeneratedAtUtc],
        [OwnerName]
FROM [Analytics].[Reporting]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@ReportType IS NULL OR [ReportCategory] = @ReportType)
ORDER BY [GeneratedAtUtc] DESC, [ReportItemId] ASC
OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

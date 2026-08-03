-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Reporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Reporting_GetListCount.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Total row count for the paginated/type-filtered report catalog query,
--                  backing IReportingRepository.GetReportsPagedAsync. Split from
--                  usp_Reporting_GetListPage (rather than a single multi-result-set
--                  procedure) because the shared DapperQueryExecutor exposes single-result-set
--                  helpers only. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Reporting_GetListCount]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @ReportType  NVARCHAR(100) = NULL
AS
SET NOCOUNT ON;

SELECT COUNT(*)
FROM [Analytics].[Reporting]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@ReportType IS NULL OR [ReportCategory] = @ReportType);
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Dashboard_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all Dashboard rows. Backs IDashboardRepository.ListAsync, whose
--                  DashboardSpecification predicate is applied in-memory over the candidate set
--                  returned here. Mirrors usp_Permits_GetAllActive.sql. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        DashboardId, TenantId, SiteId, Status, DashboardName, DashboardType, OwnerId,
        RefreshIntervalSeconds, WidgetCount, IsPublished, LastRefreshedAtUtc,
        PublishedByUserId, PublishedAtUtc, DeprecatedByUserId, DeprecatedAtUtc,
        ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy,
        UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[Dashboard];
END
GO

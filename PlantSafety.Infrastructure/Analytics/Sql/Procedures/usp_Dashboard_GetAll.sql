-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Dashboard_GetAll.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Tenant-scoped read-only stored procedure. Backs the pre-existing, previously
--                  unimplemented Infrastructure.Analytics.Dashboard.Persistence.DashboardRepository
--                  .GetAllAsync(tenantId) call site (that class referenced this proc name before
--                  it existed anywhere in the codebase). Mirrors usp_Culture_GetAll.sql.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_GetAll]
    @TenantId UNIQUEIDENTIFIER
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
    FROM [Analytics].[Dashboard]
    WHERE TenantId = @TenantId
    ORDER BY CreatedAtUtc DESC;
END
GO

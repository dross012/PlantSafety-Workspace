-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.ReadModel
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ReadModel_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[ReadModel],
--                  backing IReadModelRepository.GetDashboardRowsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_ReadModel_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RowId],
        [TenantId],
        [SiteId],
        [WidgetName],
        [WidgetValue],
        [DisplayOrder],
        [Status],
        [RefreshedAtUtc]
FROM [Analytics].[ReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RefreshedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [DisplayOrder] ASC, [RefreshedAtUtc] DESC;
GO

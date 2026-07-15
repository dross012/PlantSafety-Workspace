-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.KPISnapshots
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_KPISnapshots_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[KPISnapshots],
--                  backing IKPISnapshotsRepository.GetKpiSnapshotsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_KPISnapshots_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [SnapshotId],
        [TenantId],
        [SiteId],
        [KpiName],
        [KpiValue],
        [UnitOfMeasure],
        [Status],
        [SnapshotAtUtc]
FROM [Analytics].[KPISnapshots]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [SnapshotAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [SnapshotAtUtc] DESC;
GO

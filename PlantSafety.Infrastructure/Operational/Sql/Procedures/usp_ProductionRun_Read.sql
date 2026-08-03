-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionRun_Read.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Application-layer read-side query procedures for ProductionRun: paginated
--                  active-runs list and the un-paginated active-runs KPI/telemetry-input feed
--                  consumed by GetThroughputSummaryQuery/GetBottleneckSummaryQuery/GetPlantKpisQuery
--                  (see ProductionTrackingKpiQueryHandlers.cs). Every paginated proc carries a
--                  COUNT(*) OVER() window-function TotalCount, mirroring usp_WorkOrder_List.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_ListActive]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    ProductionRunId, RunCode, LineName, MachineId, StartedAtUtc, EndedAtUtc, Status,
    GoodUnitCount, ScrapUnitCount, IdealCycleTimeSeconds,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRuns]
WHERE TenantId = @TenantId
  AND IsDeleted = 0
  AND Status IN (N'Running', N'Paused')
ORDER BY StartedAtUtc DESC, ProductionRunId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_ActiveKpiBase]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    ProductionRunId, RunCode, MachineId, StartedAtUtc, EndedAtUtc,
    GoodUnitCount, ScrapUnitCount, IdealCycleTimeSeconds, DowntimeMinutesTotal
FROM [Operational].[ProductionRuns]
WHERE TenantId = @TenantId
  AND IsDeleted = 0
  AND Status IN (N'Running', N'Paused')
ORDER BY StartedAtUtc ASC;
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionTracking_PlantWide.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Plant-wide (cross-run) chronological timeline and searchable event-history log
--                  procedures, backing GetProductionTimelineQuery/GetEventHistoryQuery.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionTimeline_PlantWide]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc AS TimestampUtc, RunEventType AS EventType, ISNULL(Description, N'') AS Description, ProductionRunId,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRunEvents]
WHERE TenantId = @TenantId
ORDER BY OccurredAtUtc DESC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionEventHistory]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc AS TimestampUtc, RunCode, ProductionRunId, RunEventType AS EventType, ISNULL(Description, N'') AS Description,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRunEvents]
WHERE TenantId = @TenantId
ORDER BY OccurredAtUtc DESC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

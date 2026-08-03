-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionRunEvent_Downtime.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Insert procedures for ProductionRunEvents (timeline)/ProductionDowntimeEvents,
--                  plus the by-run downtime list feeding the real
--                  Observability.Telemetry.TelemetryAnomalyDetector-backed bottleneck detection
--                  read path (see ProductionTrackingKpiQueryHandlers.cs). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRunEvent_Insert]
    @EventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @RunCode NVARCHAR(50),
    @RunEventType NVARCHAR(50),
    @Description NVARCHAR(2000) = NULL,
    @OccurredAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;

INSERT INTO [Operational].[ProductionRunEvents]
    (EventId, TenantId, ProductionRunId, RunCode, RunEventType, Description, OccurredAtUtc)
VALUES
    (@EventId, @TenantId, @ProductionRunId, @RunCode, @RunEventType, @Description, @OccurredAtUtc);
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionDowntimeEvent_Insert]
    @EventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @MachineId UNIQUEIDENTIFIER,
    @ReasonCode NVARCHAR(50),
    @StartUtc DATETIME2(3),
    @EndUtc DATETIME2(3) = NULL,
    @IsPlanned BIT = 0
AS
SET NOCOUNT ON;

INSERT INTO [Operational].[ProductionDowntimeEvents]
    (EventId, TenantId, ProductionRunId, MachineId, ReasonCode, StartUtc, EndUtc, IsPlanned)
VALUES
    (@EventId, @TenantId, @ProductionRunId, @MachineId, @ReasonCode, @StartUtc, @EndUtc, @IsPlanned);
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionDowntimeEvent_ListByRun]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT ReasonCode, StartUtc, EndUtc, IsPlanned
FROM [Operational].[ProductionDowntimeEvents]
WHERE TenantId = @TenantId
  AND ProductionRunId = @ProductionRunId
ORDER BY StartUtc ASC;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Timeline]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
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
  AND ProductionRunId = @ProductionRunId
ORDER BY OccurredAtUtc ASC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Events]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc AS TimestampUtc, RunCode, ProductionRunId, RunEventType AS EventType, ISNULL(Description, N'') AS Description,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRunEvents]
WHERE TenantId = @TenantId
  AND ProductionRunId = @ProductionRunId
ORDER BY OccurredAtUtc DESC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

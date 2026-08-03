-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_Calendar.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Maintenance calendar events within an inclusive UTC date range, projected from
--                  WorkOrder.DueAtUtc with a nominal 4-hour work window. Backs
--                  GetMaintenanceCalendarQuery (v1/maintenance/calendar). New-build scope -- see
--                  MaintenanceCalendarEventDto remarks for the future Scheduling-module rollup
--                  layer this is designed to support. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_Calendar]
    @TenantId UNIQUEIDENTIFIER,
    @RangeStartUtc DATETIME2(3),
    @RangeEndUtc DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
    WorkOrderId AS EventId,
    WorkOrderCode,
    MachineCode AS EquipmentName,
    TechnicianName,
    DueAtUtc AS StartUtc,
    DATEADD(HOUR, 4, DueAtUtc) AS EndUtc,
    N'Maintenance' AS EventType,
    CASE Status
        WHEN N'Completed' THEN N'Completed'
        WHEN N'Cancelled' THEN N'Cancelled'
        WHEN N'InProgress' THEN N'InProgress'
        WHEN N'OnHold' THEN N'InProgress'
        ELSE N'Planned'
    END AS Status,
    Priority
FROM [Operational].[WorkOrders]
WHERE TenantId = @TenantId
  AND IsDeleted = 0
  AND DueAtUtc BETWEEN @RangeStartUtc AND @RangeEndUtc
ORDER BY DueAtUtc ASC;
GO

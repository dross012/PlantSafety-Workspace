-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_MachineStatusSummary.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Machine Status Summary card counts, derived heuristically from open work-order
--                  priority per distinct MachineId (no Machine/Equipment master aggregate exists in
--                  the platform yet -- documented assumption). Offline = at least one open Critical
--                  work order; Degraded = at least one open non-Critical work order and no open
--                  Critical one; Healthy = every work order for that machine is Completed/Cancelled.
--                  Backs GetMachineStatusSummaryQuery (v1/maintenance/machines/status-summary).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_MachineStatusSummary]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

WITH MachineRollup AS (
    SELECT
        MachineId,
        MAX(CASE WHEN Status NOT IN (N'Completed', N'Cancelled') AND Priority = N'Critical' THEN 1 ELSE 0 END) AS HasOpenCritical,
        MAX(CASE WHEN Status NOT IN (N'Completed', N'Cancelled') THEN 1 ELSE 0 END) AS HasOpen
    FROM [Operational].[WorkOrders]
    WHERE TenantId = @TenantId AND IsDeleted = 0
    GROUP BY MachineId
)
SELECT
    SUM(CASE WHEN HasOpen = 0 THEN 1 ELSE 0 END) AS HealthyCount,
    SUM(CASE WHEN HasOpen = 1 AND HasOpenCritical = 0 THEN 1 ELSE 0 END) AS DegradedCount,
    SUM(CASE WHEN HasOpenCritical = 1 THEN 1 ELSE 0 END) AS OfflineCount
FROM MachineRollup;
GO

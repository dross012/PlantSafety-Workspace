-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_Tasks.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Ordered task/step rows for a work order. Backs GetWorkOrderTasksQuery
--                  (v1/maintenance/work-orders/{id}/tasks). See WorkOrderTasks.sql remarks -- no
--                  writer populates this table yet, so this returns an empty set until a future
--                  "AddWorkOrderTaskStep" command is added. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_Tasks]
    @TenantId UNIQUEIDENTIFIER,
    @WorkOrderId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    t.StepNumber, t.Description, t.DurationMinutes, t.Status, t.TechnicianNotes,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[WorkOrderTasks] t
INNER JOIN [Operational].[WorkOrders] w ON w.WorkOrderId = t.WorkOrderId
WHERE w.TenantId = @TenantId AND w.WorkOrderId = @WorkOrderId AND w.IsDeleted = 0
ORDER BY t.StepNumber ASC;
GO

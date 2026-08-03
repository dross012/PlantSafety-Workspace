-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkOrderTasks.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Child table of task/step rows within a WorkOrder. Backs
--                  GetWorkOrderTasksQuery (v1/maintenance/work-orders/{id}/tasks). New-build scope:
--                  no command populates this table yet (task-step tracking is out of the literal
--                  command surface listed for this pass -- Create/Assign/UpdateStatus/Complete work
--                  order + AdjustPartStock). The table and read path exist so the sub-resource
--                  degrades gracefully (returns an empty page) rather than erroring, and so a future
--                  "AddWorkOrderTaskStep" command can be added additively without a schema change.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkOrderTasks' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WorkOrderTasks] (
        [WorkOrderId]       UNIQUEIDENTIFIER NOT NULL,
        [StepNumber]        INT              NOT NULL,
        [Description]       NVARCHAR(2000)   NOT NULL,
        [DurationMinutes]   INT              NOT NULL,
        [Status]            NVARCHAR(50)     NOT NULL,
        [TechnicianNotes]   NVARCHAR(2000)   NULL,
        CONSTRAINT PK_WorkOrderTasks PRIMARY KEY ([WorkOrderId], [StepNumber]),
        CONSTRAINT FK_WorkOrderTasks_WorkOrders FOREIGN KEY ([WorkOrderId]) REFERENCES [Operational].[WorkOrders] ([WorkOrderId])
    );
END
GO

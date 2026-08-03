-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkOrderPartUsage.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Child table recording parts consumed by a WorkOrder, distinct from the Parts
--                  inventory master. Backs GetWorkOrderPartsUsageQuery
--                  (v1/maintenance/work-orders/{id}/parts). New-build scope: no command populates
--                  this table yet (see WorkOrderTasks.sql remarks for the same rationale -- the
--                  sub-resource degrades gracefully to an empty page). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkOrderPartUsage' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WorkOrderPartUsage] (
        [WorkOrderPartUsageId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [WorkOrderId]          UNIQUEIDENTIFIER NOT NULL,
        [PartId]               UNIQUEIDENTIFIER NOT NULL,
        [PartNumber]           NVARCHAR(50)     NOT NULL,
        [PartName]             NVARCHAR(200)    NOT NULL,
        [QuantityUsed]         INT              NOT NULL,
        [WasAvailable]         BIT              NOT NULL,
        CONSTRAINT FK_WorkOrderPartUsage_WorkOrders FOREIGN KEY ([WorkOrderId]) REFERENCES [Operational].[WorkOrders] ([WorkOrderId])
    );

    CREATE NONCLUSTERED INDEX [IX_WorkOrderPartUsage_WorkOrderId] ON [Operational].[WorkOrderPartUsage] ([WorkOrderId]);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           MaintenanceEvents.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Chronological event log for WorkOrder lifecycle transitions. Populated by
--                  WorkOrderDapperRepository.AddAsync/UpdateAsync alongside the outbox write (one
--                  row per domain event raised), so GetWorkOrderHistoryQuery and
--                  GetMachineHistoryQuery are backed by genuine data from day one rather than an
--                  empty placeholder table. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceEvents' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[MaintenanceEvents] (
        [EventId]         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]        UNIQUEIDENTIFIER NOT NULL,
        [WorkOrderId]     UNIQUEIDENTIFIER NOT NULL,
        [WorkOrderCode]   NVARCHAR(50)     NOT NULL,
        [MachineId]       UNIQUEIDENTIFIER NOT NULL,
        [OccurredAtUtc]   DATETIME2(3)     NOT NULL,
        [TechnicianName]  NVARCHAR(200)    NULL,
        [Summary]         NVARCHAR(500)    NOT NULL,
        CONSTRAINT FK_MaintenanceEvents_WorkOrders FOREIGN KEY ([WorkOrderId]) REFERENCES [Operational].[WorkOrders] ([WorkOrderId])
    );

    CREATE NONCLUSTERED INDEX [IX_MaintenanceEvents_TenantId_WorkOrderId] ON [Operational].[MaintenanceEvents] ([TenantId], [WorkOrderId]);
    CREATE NONCLUSTERED INDEX [IX_MaintenanceEvents_TenantId_MachineId] ON [Operational].[MaintenanceEvents] ([TenantId], [MachineId]);
END
GO

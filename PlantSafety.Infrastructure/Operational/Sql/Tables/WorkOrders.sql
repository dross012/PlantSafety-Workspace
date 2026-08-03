-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkOrders.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WorkOrder aggregate root. New-build module backing
--                  v1/maintenance/work-orders* (see docs/Phase5/ui-gaps-scoping.md "Maintenance"
--                  section). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Operational')
BEGIN
    EXEC('CREATE SCHEMA [Operational]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkOrders' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WorkOrders] (
        [WorkOrderId]       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [Status]            NVARCHAR(50)     NOT NULL,
        [WorkOrderCode]     NVARCHAR(50)     NOT NULL,
        [MachineId]         UNIQUEIDENTIFIER NOT NULL,
        [MachineCode]       NVARCHAR(50)     NOT NULL,
        [Priority]          NVARCHAR(20)     NOT NULL,
        [Description]       NVARCHAR(2000)   NOT NULL,
        [DueAtUtc]          DATETIME2(3)     NOT NULL,
        [TechnicianId]      UNIQUEIDENTIFIER NULL,
        [TechnicianName]    NVARCHAR(200)    NULL,
        [AssignedAtUtc]     DATETIME2(3)     NULL,
        [StartedAtUtc]      DATETIME2(3)     NULL,
        [OnHoldAtUtc]       DATETIME2(3)     NULL,
        [CompletedBy]       UNIQUEIDENTIFIER NULL,
        [CompletionNotes]   NVARCHAR(2000)   NULL,
        [CompletedAtUtc]    DATETIME2(3)     NULL,
        [CancelledBy]       UNIQUEIDENTIFIER NULL,
        [CancelledReason]   NVARCHAR(2000)   NULL,
        [CancelledAtUtc]    DATETIME2(3)     NULL,
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER NOT NULL,
        [UpdatedAtUtc]      DATETIME2(3)     NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT DF_WorkOrders_IsDeleted DEFAULT (0),
        [RowVersion]        ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WorkOrders_TenantId_Status] ON [Operational].[WorkOrders] ([TenantId], [Status]) INCLUDE ([MachineId], [DueAtUtc]);
    CREATE NONCLUSTERED INDEX [IX_WorkOrders_TenantId_MachineId] ON [Operational].[WorkOrders] ([TenantId], [MachineId]);
    CREATE NONCLUSTERED INDEX [IX_WorkOrders_TenantId_DueAtUtc] ON [Operational].[WorkOrders] ([TenantId], [DueAtUtc]);
END
GO

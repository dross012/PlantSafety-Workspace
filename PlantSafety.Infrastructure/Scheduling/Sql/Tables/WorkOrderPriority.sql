-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           WorkOrderPriority.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the WorkOrderPriority aggregate root (IWorkOrderPriorityRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkOrderPriority' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[WorkOrderPriority]
    (
        WorkOrderPriorityId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WorkOrderPriority PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PriorityCode NVARCHAR(400) NOT NULL,
        HazardDescription NVARCHAR(400) NOT NULL,
        DueByUtc DATETIME2(3) NULL,
        EscalatedAtUtc DATETIME2(3) NULL,
        ResolutionNotes NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_WorkOrderPriority_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_WorkOrderPriority_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_WorkOrderPriority_TenantId_Site ON [Scheduling].[WorkOrderPriority] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_WorkOrderPriority_Status ON [Scheduling].[WorkOrderPriority] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_WorkOrderPriority_CreatedAtUtc ON [Scheduling].[WorkOrderPriority] (CreatedAtUtc);
END
GO

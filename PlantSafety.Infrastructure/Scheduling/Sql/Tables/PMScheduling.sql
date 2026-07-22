-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PMScheduling.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PMScheduling aggregate root (IPMSchedulingRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PMScheduling' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[PMScheduling]
    (
        PMSchedulingId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PMScheduling PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        EquipmentId UNIQUEIDENTIFIER NOT NULL,
        EquipmentDescription NVARCHAR(400) NOT NULL,
        PmTaskType NVARCHAR(400) NOT NULL,
        DueAtUtc DATETIME2(3) NULL,
        IsOverdue BIT NOT NULL,
        EscalationPriorityCode NVARCHAR(400) NULL,
        CompletionNotes NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PMScheduling_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PMScheduling_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PMScheduling_TenantId_Site ON [Scheduling].[PMScheduling] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PMScheduling_Status ON [Scheduling].[PMScheduling] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PMScheduling_CreatedAtUtc ON [Scheduling].[PMScheduling] (CreatedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           MachineProcessMonitoring.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the MachineProcessMonitoring aggregate root (IMachineProcessMonitoringRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Observability')
    EXEC('CREATE SCHEMA Observability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MachineProcessMonitoring' AND schema_id = SCHEMA_ID('Observability'))
BEGIN
    CREATE TABLE [Observability].[MachineProcessMonitoring]
    (
        MachineProcessMonitoringId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MachineProcessMonitoring PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        MachineId UNIQUEIDENTIFIER NOT NULL,
        SensorType NVARCHAR(400) NOT NULL,
        Reading DECIMAL(18,4) NOT NULL,
        ThresholdBreached BIT NOT NULL,
        AcknowledgedBy UNIQUEIDENTIFIER NULL,
        AnomalyDescription NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_MachineProcessMonitoring_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_MachineProcessMonitoring_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_MachineProcessMonitoring_TenantId_Site ON [Observability].[MachineProcessMonitoring] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_MachineProcessMonitoring_Status ON [Observability].[MachineProcessMonitoring] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_MachineProcessMonitoring_CreatedAtUtc ON [Observability].[MachineProcessMonitoring] (CreatedAtUtc);
END
GO

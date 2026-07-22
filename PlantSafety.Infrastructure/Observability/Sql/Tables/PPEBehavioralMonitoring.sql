-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPEBehavioralMonitoring.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPEBehavioralMonitoring aggregate root (IPPEBehavioralMonitoringRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Observability')
    EXEC('CREATE SCHEMA Observability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEBehavioralMonitoring' AND schema_id = SCHEMA_ID('Observability'))
BEGIN
    CREATE TABLE [Observability].[PPEBehavioralMonitoring]
    (
        PPEBehavioralMonitoringId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPEBehavioralMonitoring PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        SensorId NVARCHAR(400) NOT NULL,
        ZoneCode NVARCHAR(400) NOT NULL,
        DetectedEventType NVARCHAR(400) NOT NULL,
        ConfidenceScore DECIMAL(18,4) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPEBehavioralMonitoring_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPEBehavioralMonitoring_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPEBehavioralMonitoring_TenantId_Site ON [Observability].[PPEBehavioralMonitoring] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEBehavioralMonitoring_Status ON [Observability].[PPEBehavioralMonitoring] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEBehavioralMonitoring_CreatedAtUtc ON [Observability].[PPEBehavioralMonitoring] (CreatedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           EnvironmentalAssetMonitoring.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the EnvironmentalAssetMonitoring aggregate root (IEnvironmentalAssetMonitoringRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Observability')
    EXEC('CREATE SCHEMA Observability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnvironmentalAssetMonitoring' AND schema_id = SCHEMA_ID('Observability'))
BEGIN
    CREATE TABLE [Observability].[EnvironmentalAssetMonitoring]
    (
        EnvironmentalAssetMonitoringId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnvironmentalAssetMonitoring PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        SensorId NVARCHAR(400) NOT NULL,
        MeasurementType NVARCHAR(400) NOT NULL,
        Value DECIMAL(18,4) NOT NULL,
        ThresholdExceeded BIT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_EnvironmentalAssetMonitoring_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EnvironmentalAssetMonitoring_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_EnvironmentalAssetMonitoring_TenantId_Site ON [Observability].[EnvironmentalAssetMonitoring] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnvironmentalAssetMonitoring_Status ON [Observability].[EnvironmentalAssetMonitoring] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnvironmentalAssetMonitoring_CreatedAtUtc ON [Observability].[EnvironmentalAssetMonitoring] (CreatedAtUtc);
END
GO

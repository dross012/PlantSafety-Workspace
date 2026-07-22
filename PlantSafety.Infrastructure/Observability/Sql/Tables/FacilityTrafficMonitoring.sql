-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FacilityTrafficMonitoring.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the FacilityTrafficMonitoring aggregate root (IFacilityTrafficMonitoringRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Observability')
    EXEC('CREATE SCHEMA Observability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FacilityTrafficMonitoring' AND schema_id = SCHEMA_ID('Observability'))
BEGIN
    CREATE TABLE [Observability].[FacilityTrafficMonitoring]
    (
        FacilityTrafficMonitoringId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FacilityTrafficMonitoring PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        ZoneCode NVARCHAR(400) NOT NULL,
        EventType NVARCHAR(400) NOT NULL,
        SeverityScore DECIMAL(18,4) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_FacilityTrafficMonitoring_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FacilityTrafficMonitoring_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_FacilityTrafficMonitoring_TenantId_Site ON [Observability].[FacilityTrafficMonitoring] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_FacilityTrafficMonitoring_Status ON [Observability].[FacilityTrafficMonitoring] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_FacilityTrafficMonitoring_CreatedAtUtc ON [Observability].[FacilityTrafficMonitoring] (CreatedAtUtc);
END
GO

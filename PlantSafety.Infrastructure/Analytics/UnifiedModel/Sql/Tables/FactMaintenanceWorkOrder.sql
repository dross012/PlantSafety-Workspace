-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactMaintenanceWorkOrder.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the Maintenance domain of the unified analytics data model.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactMaintenanceWorkOrder' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder]
    (
        FactId                       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactMaintenanceWorkOrder PRIMARY KEY,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        SiteKey                      UNIQUEIDENTIFIER NULL,
        DateKey                      UNIQUEIDENTIFIER NOT NULL,
        AssetKey                     UNIQUEIDENTIFIER NOT NULL,
        EmployeeKey                  UNIQUEIDENTIFIER NULL,
        MaintenanceType               NVARCHAR(20)     NOT NULL,
        Status                        NVARCHAR(30)     NOT NULL,
        DueAtUtc                      DATETIME2(3)     NOT NULL,
        CompletedAtUtc                DATETIME2(3)     NULL,
        DowntimeMinutes                INT              NOT NULL CONSTRAINT DF_FactMaintenanceWorkOrder_Downtime DEFAULT (0),
        PredictedFailureProbability   FLOAT            NULL,
        WasOnSchedule                  BIT              NOT NULL CONSTRAINT DF_FactMaintenanceWorkOrder_OnSchedule DEFAULT (0),
        CapturedAtUtc                  DATETIME2(3)     NOT NULL,
        SourceAggregateId              UNIQUEIDENTIFIER NOT NULL,
        SourceModule                   NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactMaintenanceWorkOrder_Module DEFAULT ('Maintenance')
    );

    CREATE NONCLUSTERED INDEX IX_FactMaintenanceWorkOrder_Tenant_Date ON [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactMaintenanceWorkOrder_Asset ON [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] (AssetKey);
    CREATE NONCLUSTERED INDEX IX_FactMaintenanceWorkOrder_Type_Status ON [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] (MaintenanceType, Status);
END
GO

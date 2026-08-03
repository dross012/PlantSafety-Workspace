-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactSchedulingEvent.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the Scheduling domain of the unified analytics data model.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactSchedulingEvent' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactSchedulingEvent]
    (
        FactId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactSchedulingEvent PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteKey                UNIQUEIDENTIFIER NULL,
        DateKey                UNIQUEIDENTIFIER NOT NULL,
        AssetKey               UNIQUEIDENTIFIER NULL,
        EmployeeKey             UNIQUEIDENTIFIER NULL,
        ShiftKey                UNIQUEIDENTIFIER NULL,
        SchedulingSubContext    NVARCHAR(60)     NOT NULL,
        EventType               NVARCHAR(60)     NOT NULL,
        ScheduledAtUtc          DATETIME2(3)     NOT NULL,
        CompletedAtUtc          DATETIME2(3)     NULL,
        VarianceMinutes         INT              NOT NULL CONSTRAINT DF_FactSchedulingEvent_Variance DEFAULT (0),
        WasOnTime               BIT              NOT NULL CONSTRAINT DF_FactSchedulingEvent_OnTime DEFAULT (0),
        PriorityCode            NVARCHAR(10)     NULL,
        CapturedAtUtc           DATETIME2(3)     NOT NULL,
        SourceAggregateId       UNIQUEIDENTIFIER NOT NULL,
        SourceModule            NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactSchedulingEvent_Module DEFAULT ('Scheduling')
    );

    CREATE NONCLUSTERED INDEX IX_FactSchedulingEvent_Tenant_Date ON [AnalyticsUnifiedModel].[FactSchedulingEvent] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactSchedulingEvent_Tenant_Site ON [AnalyticsUnifiedModel].[FactSchedulingEvent] (TenantId, SiteKey);
    CREATE NONCLUSTERED INDEX IX_FactSchedulingEvent_SubContext ON [AnalyticsUnifiedModel].[FactSchedulingEvent] (SchedulingSubContext, EventType);
END
GO

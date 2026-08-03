-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactOperationalEvent.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the Production/Operational domain of the unified analytics
--                  data model. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactOperationalEvent' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactOperationalEvent]
    (
        FactId                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactOperationalEvent PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteKey                 UNIQUEIDENTIFIER NULL,
        DateKey                 UNIQUEIDENTIFIER NOT NULL,
        AssetKey                UNIQUEIDENTIFIER NULL,
        EmployeeKey             UNIQUEIDENTIFIER NULL,
        OperationalSubContext   NVARCHAR(60)     NOT NULL,
        EventType               NVARCHAR(60)     NOT NULL,
        OccurredAtUtc           DATETIME2(3)     NOT NULL,
        OutcomeCode              NVARCHAR(30)     NULL,
        DiscrepancyCount         INT              NOT NULL CONSTRAINT DF_FactOperationalEvent_Discrepancy DEFAULT (0),
        IsCompliant              BIT              NOT NULL CONSTRAINT DF_FactOperationalEvent_Compliant DEFAULT (1),
        CapturedAtUtc            DATETIME2(3)     NOT NULL,
        SourceAggregateId        UNIQUEIDENTIFIER NOT NULL,
        SourceModule             NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactOperationalEvent_Module DEFAULT ('Operational')
    );

    CREATE NONCLUSTERED INDEX IX_FactOperationalEvent_Tenant_Date ON [AnalyticsUnifiedModel].[FactOperationalEvent] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactOperationalEvent_Tenant_Site ON [AnalyticsUnifiedModel].[FactOperationalEvent] (TenantId, SiteKey);
    CREATE NONCLUSTERED INDEX IX_FactOperationalEvent_SubContext ON [AnalyticsUnifiedModel].[FactOperationalEvent] (OperationalSubContext, EventType);
END
GO

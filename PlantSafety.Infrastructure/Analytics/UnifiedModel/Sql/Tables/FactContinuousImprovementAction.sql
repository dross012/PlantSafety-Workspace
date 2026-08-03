-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactContinuousImprovementAction.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the ContinuousImprovement domain of the unified analytics
--                  data model. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactContinuousImprovementAction' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactContinuousImprovementAction]
    (
        FactId                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactContinuousImprovementAction PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteKey                 UNIQUEIDENTIFIER NULL,
        DateKey                 UNIQUEIDENTIFIER NOT NULL,
        EmployeeKey              UNIQUEIDENTIFIER NULL,
        ImprovementSubContext    NVARCHAR(20)     NOT NULL,
        ActionType                NVARCHAR(60)     NOT NULL,
        Status                    NVARCHAR(30)     NOT NULL,
        OpenedAtUtc               DATETIME2(3)     NOT NULL,
        ClosedAtUtc               DATETIME2(3)     NULL,
        CycleTimeDays             INT              NULL,
        WasClosedOnTime           BIT              NOT NULL CONSTRAINT DF_FactCIAction_OnTime DEFAULT (0),
        EffectivenessVerified     BIT              NOT NULL CONSTRAINT DF_FactCIAction_EffVerified DEFAULT (0),
        CapturedAtUtc             DATETIME2(3)     NOT NULL,
        SourceAggregateId         UNIQUEIDENTIFIER NOT NULL,
        SourceModule              NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactCIAction_Module DEFAULT ('ContinuousImprovement')
    );

    CREATE NONCLUSTERED INDEX IX_FactCIAction_Tenant_Date ON [AnalyticsUnifiedModel].[FactContinuousImprovementAction] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactCIAction_SubContext ON [AnalyticsUnifiedModel].[FactContinuousImprovementAction] (ImprovementSubContext, Status);
END
GO

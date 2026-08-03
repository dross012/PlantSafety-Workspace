-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactRiskAssessment.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the RiskManagement domain of the unified analytics data model.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactRiskAssessment' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactRiskAssessment]
    (
        FactId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactRiskAssessment PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteKey              UNIQUEIDENTIFIER NULL,
        DateKey              UNIQUEIDENTIFIER NOT NULL,
        EmployeeKey          UNIQUEIDENTIFIER NULL,
        Severity             INT              NOT NULL,
        Likelihood           INT              NOT NULL,
        CompositeScore       INT              NOT NULL,
        RiskLevelCode        NVARCHAR(20)     NOT NULL,
        PriorityCode         NVARCHAR(10)     NULL,
        ControlMeasureCount  INT              NOT NULL CONSTRAINT DF_FactRiskAssessment_ControlCount DEFAULT (0),
        IsResidualScore      BIT              NOT NULL CONSTRAINT DF_FactRiskAssessment_Residual DEFAULT (0),
        Status                NVARCHAR(30)     NOT NULL,
        CapturedAtUtc         DATETIME2(3)     NOT NULL,
        SourceAggregateId     UNIQUEIDENTIFIER NOT NULL,
        SourceModule          NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactRiskAssessment_Module DEFAULT ('RiskManagement')
    );

    CREATE NONCLUSTERED INDEX IX_FactRiskAssessment_Tenant_Date ON [AnalyticsUnifiedModel].[FactRiskAssessment] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactRiskAssessment_Source ON [AnalyticsUnifiedModel].[FactRiskAssessment] (SourceAggregateId);
    CREATE NONCLUSTERED INDEX IX_FactRiskAssessment_Level ON [AnalyticsUnifiedModel].[FactRiskAssessment] (RiskLevelCode);
END
GO

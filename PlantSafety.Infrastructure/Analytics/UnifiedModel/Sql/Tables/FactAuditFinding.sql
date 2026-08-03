-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FactAuditFinding.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Fact table for the Auditing domain of the unified analytics data model.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FactAuditFinding' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[FactAuditFinding]
    (
        FactId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FactAuditFinding PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteKey                UNIQUEIDENTIFIER NULL,
        DateKey                UNIQUEIDENTIFIER NOT NULL,
        AuditEngagementId       UNIQUEIDENTIFIER NOT NULL,
        EmployeeKey             UNIQUEIDENTIFIER NULL,
        SeverityCode            NVARCHAR(20)     NOT NULL,
        Category                NVARCHAR(60)     NOT NULL,
        Status                  NVARCHAR(30)     NOT NULL,
        RaisedAtUtc             DATETIME2(3)     NOT NULL,
        ClosedAtUtc             DATETIME2(3)     NULL,
        RemediationDays         INT              NULL,
        WasRemediatedOnTime     BIT              NOT NULL CONSTRAINT DF_FactAuditFinding_OnTime DEFAULT (0),
        CapturedAtUtc            DATETIME2(3)     NOT NULL,
        SourceAggregateId        UNIQUEIDENTIFIER NOT NULL,
        SourceModule              NVARCHAR(60)     NOT NULL CONSTRAINT DF_FactAuditFinding_Module DEFAULT ('Auditing')
    );

    CREATE NONCLUSTERED INDEX IX_FactAuditFinding_Tenant_Date ON [AnalyticsUnifiedModel].[FactAuditFinding] (TenantId, DateKey);
    CREATE NONCLUSTERED INDEX IX_FactAuditFinding_Engagement ON [AnalyticsUnifiedModel].[FactAuditFinding] (AuditEngagementId);
    CREATE NONCLUSTERED INDEX IX_FactAuditFinding_Severity ON [AnalyticsUnifiedModel].[FactAuditFinding] (SeverityCode, Status);
END
GO

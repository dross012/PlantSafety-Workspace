-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CyberPhysicalSecurity
--  Layer:          Infrastructure -- SQL Scripts
--  File:           SabotageIndicator.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the SabotageIndicator aggregate root (Sabotage Indicator
--                  Correlation subdomain). RiskManagementPriority backs the module's risk-hook
--                  requirement (sabotage indicators feed RiskManagement at elevated priority).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
    EXEC('CREATE SCHEMA CyberPhysicalSecurity AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SabotageIndicator' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
BEGIN
    CREATE TABLE [CyberPhysicalSecurity].[SabotageIndicator]
    (
        SabotageIndicatorId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SabotageIndicator PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NOT NULL,
        Source                   NVARCHAR(30)     NOT NULL,
        SourceReferenceId        UNIQUEIDENTIFIER NULL,
        Description              NVARCHAR(2000)   NOT NULL CONSTRAINT DF_SabotageIndicator_Desc DEFAULT (N''),
        Severity                 NVARCHAR(20)     NOT NULL,
        Status                   NVARCHAR(30)     NOT NULL,
        CorrelatedIndicatorCount INT              NOT NULL CONSTRAINT DF_SabotageIndicator_Correlated DEFAULT (1),
        RaisedAtUtc              DATETIME2(3)     NOT NULL,
        RiskManagementPriority   NVARCHAR(5)      NOT NULL,
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_SabotageIndicator_Tenant_Status ON [CyberPhysicalSecurity].[SabotageIndicator] (TenantId, Status);
    CREATE NONCLUSTERED INDEX IX_SabotageIndicator_Tenant_RaisedAt ON [CyberPhysicalSecurity].[SabotageIndicator] (TenantId, RaisedAtUtc DESC);
END
GO

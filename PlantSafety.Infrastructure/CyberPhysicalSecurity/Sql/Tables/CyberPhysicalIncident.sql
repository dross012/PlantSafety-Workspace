-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CyberPhysicalSecurity
--  Layer:          Infrastructure -- SQL Scripts
--  File:           CyberPhysicalIncident.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the CyberPhysicalIncident aggregate root (Incident Response
--                  subdomain). ResponseTimeMinutes backs the "incident response time" KPI.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
    EXEC('CREATE SCHEMA CyberPhysicalSecurity AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CyberPhysicalIncident' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
BEGIN
    CREATE TABLE [CyberPhysicalSecurity].[CyberPhysicalIncident]
    (
        CyberPhysicalIncidentId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CyberPhysicalIncident PRIMARY KEY,
        TenantId                     UNIQUEIDENTIFIER NOT NULL,
        SiteId                       UNIQUEIDENTIFIER NOT NULL,
        Title                        NVARCHAR(256)    NOT NULL,
        Description                  NVARCHAR(4000)   NOT NULL CONSTRAINT DF_CyberPhysicalIncident_Desc DEFAULT (N''),
        Severity                     NVARCHAR(20)     NOT NULL,
        Status                       NVARCHAR(20)     NOT NULL,
        RelatedSabotageIndicatorIds  NVARCHAR(4000)   NULL,
        OwnerId                      UNIQUEIDENTIFIER NOT NULL,
        OpenedAtUtc                  DATETIME2(3)     NOT NULL,
        ClosedAtUtc                  DATETIME2(3)     NULL,
        ResponseTimeMinutes          INT              NULL,
        CreatedAtUtc                 DATETIME2(3)     NOT NULL,
        CreatedBy                    UNIQUEIDENTIFIER NOT NULL,
        RowVersion                   ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CyberPhysicalIncident_Tenant_Status ON [CyberPhysicalSecurity].[CyberPhysicalIncident] (TenantId, Status);
    CREATE NONCLUSTERED INDEX IX_CyberPhysicalIncident_Tenant_OpenedAt ON [CyberPhysicalSecurity].[CyberPhysicalIncident] (TenantId, OpenedAtUtc DESC);
END
GO

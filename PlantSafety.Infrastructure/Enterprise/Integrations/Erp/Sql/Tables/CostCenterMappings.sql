-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Enterprise.Integrations.Erp
--  Layer:          Infrastructure - SQL Scripts
--  File:           CostCenterMappings.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for Tenant/Site -&gt; external ERP cost center code reference data.
--                  SiteId is nullable (a null Site means "tenant-wide default mapping"), so
--                  uniqueness is enforced via a persisted computed column
--                  (SiteIdKey = ISNULL(SiteId, the empty GUID)) rather than directly on SiteId,
--                  since SQL Server primary/unique key columns must be NOT NULL. Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Enterprise')
BEGIN
    EXEC('CREATE SCHEMA [Enterprise]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CostCenterMappings' AND schema_id = SCHEMA_ID('Enterprise'))
BEGIN
    CREATE TABLE [Enterprise].[CostCenterMappings] (
        Id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        SiteIdKey               AS ISNULL(SiteId, '00000000-0000-0000-0000-000000000000') PERSISTED NOT NULL,
        ExternalCostCenterCode  NVARCHAR(50)     NOT NULL,
        Description             NVARCHAR(200)    NULL,
        UpdatedAtUtc            DATETIME2(3)     NOT NULL,
        UpdatedBy               UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT UQ_CostCenterMappings_TenantId_SiteIdKey UNIQUE (TenantId, SiteIdKey)
    );

    CREATE NONCLUSTERED INDEX IX_CostCenterMappings_TenantId ON [Enterprise].[CostCenterMappings] (TenantId);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnterpriseFabric
--  Layer:          Infrastructure -- SQL Scripts
--  File:           EnterpriseFabricAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the EnterpriseFabric aggregate root (IEnterpriseFabricRepository,
--                  the write-side Domain repository interface). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Named "EnterpriseFabricAggregate" (not bare
--                  "EnterpriseFabric") to avoid any ambiguity with the pre-existing, unrelated
--                  [EnterpriseFabric].[FabricNode] table (Features.EnterpriseFabric.FabricNodes)
--                  that already lives in the same [EnterpriseFabric] SQL schema. Phase 4 full
--                  build-out: this aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency convention
--                  from day one.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseFabric')
    EXEC('CREATE SCHEMA EnterpriseFabric AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnterpriseFabricAggregate' AND schema_id = SCHEMA_ID('EnterpriseFabric'))
BEGIN
    CREATE TABLE [EnterpriseFabric].[EnterpriseFabricAggregate]
    (
        EnterpriseFabricId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnterpriseFabricAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        FabricArea             NVARCHAR(200)    NOT NULL,
        SystemReference        NVARCHAR(200)    NOT NULL,
        IntegrationType        NVARCHAR(100)    NOT NULL,
        OwnerUserId             UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_EnterpriseFabricAggregate_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_EnterpriseFabricAggregate_TenantId_Site ON [EnterpriseFabric].[EnterpriseFabricAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnterpriseFabricAggregate_Status ON [EnterpriseFabric].[EnterpriseFabricAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnterpriseFabricAggregate_CreatedAtUtc ON [EnterpriseFabric].[EnterpriseFabricAggregate] (CreatedAtUtc);
END
GO

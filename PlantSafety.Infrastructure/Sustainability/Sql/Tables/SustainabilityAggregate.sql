-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           SustainabilityAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Sustainability aggregate root (ISustainabilityRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "SustainabilityAggregate" (not bare "Sustainability") to avoid any ambiguity
--                  with the pre-existing, unrelated Features-vertical [Sustainability].[Metric]
--                  table that lives under this same module folder (backs the generic metric
--                  tracking Features slice, a different concept). Phase 4 full build-out: this
--                  aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency convention
--                  from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Sustainability')
    EXEC('CREATE SCHEMA Sustainability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SustainabilityAggregate' AND schema_id = SCHEMA_ID('Sustainability'))
BEGIN
    CREATE TABLE [Sustainability].[SustainabilityAggregate]
    (
        SustainabilityId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SustainabilityAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL CONSTRAINT DF_SustainabilityAggregate_Status DEFAULT (N'Created'),
        InitiativeName         NVARCHAR(400)    NOT NULL,
        Category               NVARCHAR(100)    NOT NULL,
        TargetValue            DECIMAL(18,4)    NULL,
        OwnerUserId            UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_SustainabilityAggregate_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_SustainabilityAggregate_TenantId_Site ON [Sustainability].[SustainabilityAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_SustainabilityAggregate_Status ON [Sustainability].[SustainabilityAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_SustainabilityAggregate_CreatedAtUtc ON [Sustainability].[SustainabilityAggregate] (CreatedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           RouteDefinitionAggregate.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Table backing the RouteDefinition aggregate root (IRouteDefinitionRepository).
--                  Tenant-scoped. The current, editable ordered step list is persisted as a JSON
--                  array in [StepsJson] (see RouteStepJsonModel) -- a pragmatic choice for an
--                  aggregate-owned child collection with no independent query surface of its own.
--                  Mirrors [ProcessManagement].[ProcessDefinitionAggregate]'s native ROWVERSION
--                  concurrency convention. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ProcessManagement')
    EXEC('CREATE SCHEMA ProcessManagement AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RouteDefinitionAggregate' AND schema_id = SCHEMA_ID('ProcessManagement'))
BEGIN
    CREATE TABLE [ProcessManagement].[RouteDefinitionAggregate]
    (
        RouteDefinitionId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RouteDefinitionAggregate PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        PlantName            NVARCHAR(200)    NOT NULL,
        Name                 NVARCHAR(200)    NOT NULL,
        Status               NVARCHAR(50)     NOT NULL CONSTRAINT DF_RouteDefinitionAggregate_Status DEFAULT (N'Draft'),
        Version              INT              NOT NULL CONSTRAINT DF_RouteDefinitionAggregate_Version DEFAULT (1),
        OwnerId              UNIQUEIDENTIFIER NOT NULL,
        OwnerName            NVARCHAR(200)    NOT NULL,
        EffectiveFromUtc     DATETIME2(3)     NOT NULL,
        EffectiveToUtc       DATETIME2(3)     NULL,
        StepsJson            NVARCHAR(MAX)    NOT NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        LastAuditedAtUtc     DATETIME2(3)     NOT NULL,
        UpdatedAtUtc         DATETIME2(3)     NULL,
        UpdatedBy            UNIQUEIDENTIFIER NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_RouteDefinitionAggregate_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_RouteDefinitionAggregate_TenantId_Site ON [ProcessManagement].[RouteDefinitionAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_RouteDefinitionAggregate_Status ON [ProcessManagement].[RouteDefinitionAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_RouteDefinitionAggregate_Name ON [ProcessManagement].[RouteDefinitionAggregate] (TenantId, Name, IsDeleted);
END
GO

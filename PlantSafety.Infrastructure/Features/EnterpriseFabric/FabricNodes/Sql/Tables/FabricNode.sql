-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure – SQL Scripts
--  File:           FabricNode.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the integration fabric node registry, backing
--                  IFabricNodeRepository (usp_FabricNode_Create/GetById/List).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseFabric')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseFabric]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FabricNode' AND schema_id = SCHEMA_ID('EnterpriseFabric'))
BEGIN
    CREATE TABLE [EnterpriseFabric].[FabricNode] (
        NodeId              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        NodeName            NVARCHAR(200)    NOT NULL,
        NodeType            NVARCHAR(50)     NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        LastHeartbeatAtUtc  DATETIME2(3)     NOT NULL,
        Region              NVARCHAR(50)     NOT NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_EnterpriseFabric_FabricNode_IsDeleted DEFAULT (0),
        CorrelationId       UNIQUEIDENTIFIER NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_EnterpriseFabric_FabricNode_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy           UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc       DATETIME2(3)     NULL,
        ModifiedBy          UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EnterpriseFabric_FabricNode_TenantId_SiteId] ON [EnterpriseFabric].[FabricNode] (TenantId, SiteId);
END
GO

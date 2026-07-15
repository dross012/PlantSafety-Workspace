-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure - SQL Scripts
--  File:           FabricNode.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the integration fabric node registry backing
--                  IFabricNodeRepository. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FabricNode' AND schema_id = SCHEMA_ID('EnterpriseFabric'))
BEGIN
    CREATE TABLE [EnterpriseFabric].[FabricNode] (
        [NodeId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]            UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]              UNIQUEIDENTIFIER    NULL,
        [NodeName]            NVARCHAR(200)       NOT NULL,
        [NodeType]            NVARCHAR(50)        NOT NULL,
        [Status]              NVARCHAR(50)        NOT NULL CONSTRAINT DF_FabricNode_Status DEFAULT ('Configuring'),
        [LastHeartbeatAtUtc]  DATETIME2(3)        NOT NULL,
        [Region]              NVARCHAR(50)        NOT NULL,
        [CreatedAtUtc]        DATETIME2(3)        NOT NULL,
        [CorrelationId]       UNIQUEIDENTIFIER    NOT NULL,
        [ActorId]             UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FabricNode_TenantId_SiteId]
        ON [EnterpriseFabric].[FabricNode] ([TenantId], [SiteId])
        INCLUDE ([NodeName], [NodeType], [Status], [LastHeartbeatAtUtc], [Region]);
END
GO

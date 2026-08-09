-- ============================================================================
--  Project:        Enterprise.Graph -- Phase1-Graph
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [EnterpriseGraph].[GraphNode] (mirrors
--                  EnterpriseGraphNode.cs: NodeId/NodeType/SourceId/Label/
--                  CreatedAtUtc) and [EnterpriseGraph].[GraphEdge] (mirrors
--                  EnterpriseGraphEdge.cs: FromNodeId/ToNodeId/EdgeType/
--                  CreatedAtUtc), each FK'd back to GraphNode.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GraphNode' AND schema_id = SCHEMA_ID('EnterpriseGraph'))
BEGIN
    CREATE TABLE [EnterpriseGraph].[GraphNode]
    (
        [NodeId]        UNIQUEIDENTIFIER  NOT NULL,
        [NodeType]      NVARCHAR(20)      NOT NULL,
        [SourceId]      UNIQUEIDENTIFIER  NOT NULL,
        [Label]         NVARCHAR(400)     NOT NULL,
        [CreatedAtUtc]  DATETIME2(3)      NOT NULL,

        [CreatedUtc]    DATETIME2(3)      NOT NULL CONSTRAINT DF_GraphNode_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]     NVARCHAR(128)     NOT NULL CONSTRAINT DF_GraphNode_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]     BIT               NOT NULL CONSTRAINT DF_GraphNode_IsDeleted DEFAULT (0),

        CONSTRAINT PK_GraphNode PRIMARY KEY CLUSTERED ([NodeId]),
        CONSTRAINT CK_GraphNode_NodeType CHECK ([NodeType] IN (N'Facility', N'Employee', N'Machine', N'Hazard', N'Workflow', N'Event'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GraphEdge' AND schema_id = SCHEMA_ID('EnterpriseGraph'))
BEGIN
    CREATE TABLE [EnterpriseGraph].[GraphEdge]
    (
        [GraphEdgeId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_GraphEdge_Id DEFAULT NEWSEQUENTIALID(),
        [FromNodeId]    UNIQUEIDENTIFIER  NOT NULL,
        [ToNodeId]      UNIQUEIDENTIFIER  NOT NULL,
        [EdgeType]      NVARCHAR(20)      NOT NULL,
        [CreatedAtUtc]  DATETIME2(3)      NOT NULL,

        [CreatedUtc]    DATETIME2(3)      NOT NULL CONSTRAINT DF_GraphEdge_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]     NVARCHAR(128)     NOT NULL CONSTRAINT DF_GraphEdge_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]     BIT               NOT NULL CONSTRAINT DF_GraphEdge_IsDeleted DEFAULT (0),

        CONSTRAINT PK_GraphEdge PRIMARY KEY CLUSTERED ([GraphEdgeId]),
        CONSTRAINT FK_GraphEdge_FromNode FOREIGN KEY ([FromNodeId]) REFERENCES [EnterpriseGraph].[GraphNode] ([NodeId]),
        CONSTRAINT FK_GraphEdge_ToNode FOREIGN KEY ([ToNodeId]) REFERENCES [EnterpriseGraph].[GraphNode] ([NodeId]),
        CONSTRAINT CK_GraphEdge_EdgeType CHECK ([EdgeType] IN (N'Supervises', N'Operates', N'LocatedIn', N'Causes', N'Mitigates', N'Propagates'))
    );
END
GO

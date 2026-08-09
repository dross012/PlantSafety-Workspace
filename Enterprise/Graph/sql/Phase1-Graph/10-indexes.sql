-- ============================================================================
--  Project:        Enterprise.Graph -- Phase1-Graph
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GraphNode_NodeType_SourceId' AND object_id = OBJECT_ID('EnterpriseGraph.GraphNode'))
    CREATE NONCLUSTERED INDEX IX_GraphNode_NodeType_SourceId ON [EnterpriseGraph].[GraphNode] ([NodeType], [SourceId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GraphEdge_FromNodeId' AND object_id = OBJECT_ID('EnterpriseGraph.GraphEdge'))
    CREATE NONCLUSTERED INDEX IX_GraphEdge_FromNodeId ON [EnterpriseGraph].[GraphEdge] ([FromNodeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_GraphEdge_ToNodeId' AND object_id = OBJECT_ID('EnterpriseGraph.GraphEdge'))
    CREATE NONCLUSTERED INDEX IX_GraphEdge_ToNodeId ON [EnterpriseGraph].[GraphEdge] ([ToNodeId]);
GO

-- =============================================
-- Enterprise.Graph -- Phase1-Graph
-- Script:        35-view-graph-nodedegree.sql
-- Purpose:       EnterpriseGraph.vw_GraphNode_Degree -- per-node in/out edge
--                counts, a common graph-reporting query pattern.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EnterpriseGraph].[vw_GraphNode_Degree]
AS
SELECT
    n.[NodeId], n.[NodeType], n.[SourceId], n.[Label],
    (SELECT COUNT(*) FROM [EnterpriseGraph].[GraphEdge] AS oe WHERE oe.[FromNodeId] = n.[NodeId] AND ISNULL(oe.[IsDeleted], 0) = 0) AS [OutDegree],
    (SELECT COUNT(*) FROM [EnterpriseGraph].[GraphEdge] AS ie WHERE ie.[ToNodeId] = n.[NodeId] AND ISNULL(ie.[IsDeleted], 0) = 0) AS [InDegree]
FROM [EnterpriseGraph].[GraphNode] AS n
WHERE ISNULL(n.[IsDeleted], 0) = 0;
GO

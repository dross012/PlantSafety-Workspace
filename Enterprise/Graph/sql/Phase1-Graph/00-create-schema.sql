-- ============================================================================
--  Project:        Enterprise.Graph
--  Module:         Phase1-Graph
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [EnterpriseGraph] schema -- durable storage
--                  for Enterprise.Graph.Abstractions.Nodes.EnterpriseGraphNode
--                  / Edges.EnterpriseGraphEdge (Phase D.1), currently
--                  in-memory only (built exclusively via
--                  EnterpriseGraphBuilder). NodeType/EdgeType are local,
--                  additive-only vocabularies (not reused elsewhere), kept
--                  as CHECK constraints rather than [Lookup] FKs.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note; same rationale extends
--  to every Path D/E bounded context in this task (no dedicated Enterprise*/
--  AI* database is provisioned in the canonical inventory).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnterpriseGraph')
BEGIN
    EXEC('CREATE SCHEMA [EnterpriseGraph] AUTHORIZATION dbo;');
END
GO

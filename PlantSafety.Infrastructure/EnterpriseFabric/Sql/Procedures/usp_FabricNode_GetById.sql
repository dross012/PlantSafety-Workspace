-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_FabricNode_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single fabric node by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_FabricNode_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @NodeId   UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [NodeId], [TenantId], [SiteId], [NodeName], [NodeType], [Status],
           [LastHeartbeatAtUtc], [Region]
    FROM [EnterpriseFabric].[FabricNode]
    WHERE [TenantId] = @TenantId AND [NodeId] = @NodeId;
END;
GO

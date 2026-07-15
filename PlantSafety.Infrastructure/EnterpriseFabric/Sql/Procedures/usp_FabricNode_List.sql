-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_FabricNode_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns the integration fabric node registry for a tenant.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_FabricNode_List]
(
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [NodeId], [TenantId], [SiteId], [NodeName], [NodeType], [Status],
           [LastHeartbeatAtUtc], [Region]
    FROM [EnterpriseFabric].[FabricNode]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
    ORDER BY [NodeName] ASC;
END;
GO

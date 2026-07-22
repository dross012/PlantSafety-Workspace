-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_FabricNode_List.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IFabricNodeRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_FabricNode_List]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        NodeId,
        TenantId,
        SiteId,
        NodeName,
        NodeType,
        Status,
        LastHeartbeatAtUtc,
        Region
    FROM [EnterpriseFabric].[FabricNode]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
    ORDER BY LastHeartbeatAtUtc DESC;
END
GO

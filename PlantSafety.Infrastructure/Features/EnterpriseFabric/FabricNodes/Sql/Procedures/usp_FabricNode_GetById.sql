-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.FabricNodes
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_FabricNode_GetById.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IFabricNodeRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_FabricNode_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @NodeId   UNIQUEIDENTIFIER
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
      AND NodeId = @NodeId
      AND IsDeleted = 0;
END
GO

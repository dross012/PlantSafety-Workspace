-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Twin_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns the Digital Twin asset registry for a tenant. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_List]
(
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TwinId], [TenantId], [SiteId], [AssetName], [AssetType], [Status],
           [LastSyncAtUtc], [TelemetryHealthScore]
    FROM [DigitalTwin].[Twin]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
    ORDER BY [AssetName] ASC;
END;
GO

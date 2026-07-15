-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Twin_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single Digital Twin by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @TwinId   UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TwinId], [TenantId], [SiteId], [AssetName], [AssetType], [Status],
           [LastSyncAtUtc], [TelemetryHealthScore]
    FROM [DigitalTwin].[Twin]
    WHERE [TenantId] = @TenantId AND [TwinId] = @TwinId;
END;
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RouteObstructions_GetActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all currently open (uncleared) route obstructions for a tenant,
--                  optionally scoped to a site (via the parent route's site). Backs
--                  ListActiveObstructionsQuery. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_RouteObstructions_GetActive]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    o.[RouteObstructionId], o.[EvacuationRouteId], o.[TenantId], o.[Description], o.[Severity],
    o.[RecordedAtUtc], o.[ClearedAtUtc]
FROM [DigitalTwin].[RouteObstructions] o
INNER JOIN [DigitalTwin].[EvacuationRoutes] r ON r.[EvacuationRouteId] = o.[EvacuationRouteId]
WHERE o.[TenantId] = @TenantId
  AND o.[ClearedAtUtc] IS NULL
  AND (@SiteId IS NULL OR r.[SiteId] = @SiteId)
ORDER BY o.[RecordedAtUtc] DESC;
GO

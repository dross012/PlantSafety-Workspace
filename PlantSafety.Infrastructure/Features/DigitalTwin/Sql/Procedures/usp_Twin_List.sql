-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Twin_List.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IDigitalTwinRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_List]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        TwinId,
        TenantId,
        SiteId,
        AssetName,
        AssetType,
        Status,
        LastSyncAtUtc,
        TelemetryHealthScore
    FROM [DigitalTwin].[Twin]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
    ORDER BY LastSyncAtUtc DESC;
END
GO

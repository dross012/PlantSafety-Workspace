-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Twin_GetById.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IDigitalTwinRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @TwinId   UNIQUEIDENTIFIER
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
      AND TwinId = @TwinId
      AND IsDeleted = 0;
END
GO

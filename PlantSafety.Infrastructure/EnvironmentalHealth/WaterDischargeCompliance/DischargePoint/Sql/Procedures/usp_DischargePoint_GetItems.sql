-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePoint_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only procedure backing IDischargePointRepository.ListDischargePointsBySiteAsync (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT [DischargePointId], [TenantId], [SiteId], [DischargePermitId], [PointCode], [PointName],
           [ReceivingWaterBody], [Outfall], [Status], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargePoint]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [CreatedAtUtc] DESC;
END
GO
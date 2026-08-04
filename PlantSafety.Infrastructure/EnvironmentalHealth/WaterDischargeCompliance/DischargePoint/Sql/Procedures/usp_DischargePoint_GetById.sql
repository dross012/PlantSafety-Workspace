-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePoint_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single DischargePoint row by identifier, backing IDischargePointRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_GetById]
(
    @DischargePointId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargePointId], [TenantId], [SiteId], [Status], [DischargePermitId], [PointCode], [PointName],
           [ReceivingWaterBody], [Outfall], [Latitude], [Longitude], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargePoint]
    WHERE [DischargePointId] = @DischargePointId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
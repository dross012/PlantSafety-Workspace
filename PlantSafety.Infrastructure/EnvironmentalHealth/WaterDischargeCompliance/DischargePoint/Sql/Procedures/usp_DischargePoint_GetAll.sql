-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePoint_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted DischargePoint rows, backing IDischargePointRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargePointId], [TenantId], [SiteId], [Status], [DischargePermitId], [PointCode], [PointName],
           [ReceivingWaterBody], [Outfall], [Latitude], [Longitude], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargePoint]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
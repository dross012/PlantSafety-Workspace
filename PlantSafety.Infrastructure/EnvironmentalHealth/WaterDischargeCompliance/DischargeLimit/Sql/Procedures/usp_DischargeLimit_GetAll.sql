-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeLimit_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted DischargeLimit rows, backing IDischargeLimitRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargeLimitId], [TenantId], [SiteId], [Status], [DischargePointId], [DischargePermitId], [Parameter],
           [LimitBasis], [LimitValue], [UnitOfMeasure], [EffectiveAtUtc], [PermitConditionReference], [RevisionCount],
           [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargeLimit]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
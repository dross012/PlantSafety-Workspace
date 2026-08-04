-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeLimit_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only procedure backing IDischargeLimitRepository.ListDischargeLimitsByPointAsync (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeLimit_GetItems]
    @TenantId         UNIQUEIDENTIFIER,
    @DischargePointId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT [DischargeLimitId], [TenantId], [SiteId], [DischargePointId], [Parameter], [LimitBasis], [LimitValue],
           [UnitOfMeasure], [PermitConditionReference], [Status], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargeLimit]
    WHERE [TenantId] = @TenantId
      AND [DischargePointId] = @DischargePointId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [CreatedAtUtc] DESC;
END
GO
-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Metric_GetById.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ISustainabilityRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_GetById]
    @TenantId UNIQUEIDENTIFIER,
    @MetricId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        MetricId,
        TenantId,
        SiteId,
        MetricName,
        Category,
        Value,
        Unit,
        Target,
        PeriodEndUtc,
        Status
    FROM [Sustainability].[Metric]
    WHERE TenantId = @TenantId
      AND MetricId = @MetricId
      AND IsDeleted = 0;
END
GO

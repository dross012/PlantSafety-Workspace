-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Metric_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single sustainability metric by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_GetById]
(
    @TenantId UNIQUEIDENTIFIER,
    @MetricId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MetricId], [TenantId], [SiteId], [MetricName], [Category], [Value], [Unit],
           [Target], [PeriodEndUtc], [Status]
    FROM [Sustainability].[Metric]
    WHERE [TenantId] = @TenantId AND [MetricId] = @MetricId;
END;
GO

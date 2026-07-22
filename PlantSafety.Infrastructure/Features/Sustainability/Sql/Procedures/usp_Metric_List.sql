-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Metric_List.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ISustainabilityRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_List]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
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
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND PeriodEndUtc >= @FromUtc
      AND PeriodEndUtc <= @ToUtc
    ORDER BY PeriodEndUtc DESC;
END
GO

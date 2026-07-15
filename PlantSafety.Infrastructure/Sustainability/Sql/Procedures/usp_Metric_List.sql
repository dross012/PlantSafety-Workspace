-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Metric_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns sustainability metrics for a tenant within a reporting window.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Sustainability].[usp_Metric_List]
(
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MetricId], [TenantId], [SiteId], [MetricName], [Category], [Value], [Unit],
           [Target], [PeriodEndUtc], [Status]
    FROM [Sustainability].[Metric]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [PeriodEndUtc] BETWEEN @FromUtc AND @ToUtc
    ORDER BY [PeriodEndUtc] DESC;
END;
GO

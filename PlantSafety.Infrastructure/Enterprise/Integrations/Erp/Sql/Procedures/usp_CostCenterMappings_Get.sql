-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Enterprise.Integrations.Erp
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CostCenterMappings_Get.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Retrieves the effective cost center mapping for a Tenant/Site, falling back to
--                  the tenant-wide (NULL SiteId) mapping when no site-specific mapping exists.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Enterprise].[usp_CostCenterMappings_Get]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;

SELECT TOP (1)
    [TenantId],
    [SiteId],
    [ExternalCostCenterCode],
    [Description],
    [UpdatedAtUtc],
    [UpdatedBy]
FROM [Enterprise].[CostCenterMappings]
WHERE TenantId = @TenantId
  AND (SiteId = @SiteId OR (@SiteId IS NOT NULL AND SiteId IS NULL) OR (@SiteId IS NULL AND SiteId IS NULL))
ORDER BY CASE WHEN SiteId = @SiteId THEN 0 ELSE 1 END, SiteId DESC;
GO

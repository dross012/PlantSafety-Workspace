-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SiteOrientation_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ISiteOrientationRepository.GetSiteOrientationItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_SiteOrientation_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [SiteOrientationId],
        [TenantId],
        [SiteId],
        [OrientationTitle],
        'ContractorSiteOrientation' AS [OrientationCategory],
        [ContractorName],
        [Status] AS [OrientationStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [ContractorSafety].[SiteOrientation]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

-- ============================================================================
--  Module:         LogisticsTransportSafety.Carriers
--  File:           usp_Carrier_GetItems.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing ICarrierRepository (Application read-model).
--                  GetCarrierItemsAsync, returning Carrier records for a tenant (optionally
--                  scoped to a site) created within [@FromUtc, @ToUtc]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @FromUtc DATETIME2(3),
    @ToUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CarrierId], [TenantId], [SiteId], [CarrierName], [CarrierType],
        [Status] AS [CarrierStatus], [SafetyRatingScore], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[Carriers]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [CreatedAtUtc] >= @FromUtc
      AND [CreatedAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [CreatedAtUtc] DESC;
END
GO

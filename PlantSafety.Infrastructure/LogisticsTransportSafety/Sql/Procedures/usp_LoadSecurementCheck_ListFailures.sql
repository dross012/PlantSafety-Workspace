-- ============================================================================
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  File:           usp_LoadSecurementCheck_ListFailures.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing ILoadSecurementCheckRepository (Application
--                  read-model).ListLoadSecurementFailuresAsync. Returns checks whose outcome was
--                  not a clean pass (Failed or CorrectiveActionCompleted per
--                  LoadSecurementCheckStatus) that were checked within [@FromUtc, @ToUtc], for a
--                  tenant optionally scoped to a site. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_ListFailures]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @FromUtc DATETIME2(3),
    @ToUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [LoadSecurementCheckId], [TenantId], [SiteId], [TripId], [VehicleId],
        [Status] AS [CheckStatus], [DeficiencyCount], [SecurementScore], [CheckedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[LoadSecurementChecks]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] <> N'Passed'
      AND [CheckedAtUtc] >= @FromUtc
      AND [CheckedAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [CheckedAtUtc] DESC;
END
GO

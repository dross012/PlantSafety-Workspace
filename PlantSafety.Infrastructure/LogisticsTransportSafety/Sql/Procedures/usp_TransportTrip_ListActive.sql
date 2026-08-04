-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportTrips
--  File:           usp_TransportTrip_ListActive.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing ITransportTripRepository (Application read-model).
--                  ListActiveTripsAsync (Active Trips dashboard). "Active" excludes Completed and
--                  Cancelled per TransportTripStatus (matches the usp_CommunityConcern_GetOpen
--                  NOT-IN-terminal-states precedent). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_ListActive]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [TransportTripId], [TenantId], [SiteId], [VehicleId], [DriverUserId], [TripType],
        [Status] AS [TripStatus], [RouteDescription], [PlannedDepartureAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportTrips]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] NOT IN (N'Completed', N'Cancelled')
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [PlannedDepartureAtUtc] ASC;
END
GO

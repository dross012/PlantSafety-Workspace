-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportTrips
--  File:           usp_TransportTrip_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single TransportTrip row by identifier, backing
--                  ITransportTripRepository.GetByIdAsync (Domain, id is globally unique so no
--                  tenant filter is required for correctness). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_GetById]
    @TransportTripId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
        [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [ActualDepartureAtUtc],
        [ActualArrivalAtUtc], [DistanceMiles], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportTrips]
    WHERE [TransportTripId] = @TransportTripId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

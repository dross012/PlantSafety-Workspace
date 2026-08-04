-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportTrips
--  File:           usp_TransportTrip_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted TransportTrip rows, backing
--                  ITransportTripRepository.ListAsync (the Domain specification predicate is
--                  applied in-memory in the Infrastructure repository against this full result
--                  set). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
        [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [ActualDepartureAtUtc],
        [ActualArrivalAtUtc], [DistanceMiles], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportTrips]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

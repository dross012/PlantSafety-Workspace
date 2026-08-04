-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportTrips
--  File:           usp_TransportTrip_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[TransportTrips], backing
--                  ITransportTripRepository.AddAsync (PlanTransportTrip). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_Create]
    @TransportTripId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @VehicleId UNIQUEIDENTIFIER,
    @DriverUserId UNIQUEIDENTIFIER,
    @TripType NVARCHAR(50),
    @RouteDescription NVARCHAR(1000),
    @PlannedDepartureAtUtc DATETIME2(3) = NULL,
    @PlannedArrivalAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[TransportTrips]
            ([TransportTripId], [TenantId], [SiteId], [Status], [VehicleId], [DriverUserId], [TripType],
             [RouteDescription], [PlannedDepartureAtUtc], [PlannedArrivalAtUtc], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@TransportTripId, @TenantId, @SiteId, @Status, @VehicleId, @DriverUserId, @TripType,
             @RouteDescription, @PlannedDepartureAtUtc, @PlannedArrivalAtUtc, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

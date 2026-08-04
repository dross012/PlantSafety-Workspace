-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportTrips
--  File:           usp_TransportTrip_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[TransportTrips], backing
--                  ITransportTripRepository.UpdateAsync. Covers CompleteTransportTrip and
--                  CancelTransportTrip -- matches the exact parameter set sent by
--                  TransportTripDapperRepository.UpdateAsync (Status, ActualDepartureAtUtc,
--                  ActualArrivalAtUtc, DistanceMiles; plan-time fields are immutable
--                  post-creation). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportTrip_Update]
    @NowUtc DATETIME2(3),
    @TransportTripId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @ActualDepartureAtUtc DATETIME2(3) = NULL,
    @ActualArrivalAtUtc DATETIME2(3) = NULL,
    @DistanceMiles DECIMAL(9,2) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[TransportTrips]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [ActualDepartureAtUtc] = @ActualDepartureAtUtc,
            [ActualArrivalAtUtc] = @ActualArrivalAtUtc,
            [DistanceMiles] = @DistanceMiles,
            [ModifiedAtUtc] = @NowUtc
        WHERE [TransportTripId] = @TransportTripId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

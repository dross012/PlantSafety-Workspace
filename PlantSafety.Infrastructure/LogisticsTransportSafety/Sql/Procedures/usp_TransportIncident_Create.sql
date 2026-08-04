-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportIncidents
--  File:           usp_TransportIncident_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[TransportIncidents], backing
--                  ITransportIncidentRepository.AddAsync (RecordTransportIncident). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_Create]
    @TransportIncidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @TripId UNIQUEIDENTIFIER = NULL,
    @VehicleId UNIQUEIDENTIFIER,
    @DriverUserId UNIQUEIDENTIFIER,
    @IncidentType NVARCHAR(50),
    @Severity NVARCHAR(30),
    @OccurredAtUtc DATETIME2(3) = NULL,
    @Location NVARCHAR(500),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[TransportIncidents]
            ([TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
             [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@TransportIncidentId, @TenantId, @SiteId, @Status, @TripId, @VehicleId, @DriverUserId,
             @IncidentType, @Severity, @OccurredAtUtc, @Location, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportVehicles
--  File:           usp_TransportVehicle_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[TransportVehicles], backing
--                  ITransportVehicleRepository.AddAsync (RegisterTransportVehicle). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_Create]
    @TransportVehicleId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @VehicleTitle NVARCHAR(200),
    @VehicleType NVARCHAR(50),
    @VehicleIdentifier NVARCHAR(100),
    @LastInspectionAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @LastInspectionScore DECIMAL(5,2) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[TransportVehicles]
            ([TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
             [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@TransportVehicleId, @TenantId, @SiteId, @Status, @VehicleTitle, @VehicleType, @VehicleIdentifier,
             @LastInspectionAtUtc, @NextInspectionDueAtUtc, @LastInspectionScore, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

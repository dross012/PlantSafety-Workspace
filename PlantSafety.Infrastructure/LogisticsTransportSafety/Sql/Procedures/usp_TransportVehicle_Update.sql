-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportVehicles
--  File:           usp_TransportVehicle_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[TransportVehicles], backing
--                  ITransportVehicleRepository.UpdateAsync. Covers all TransportVehicle lifecycle
--                  transitions driven through the generic UpdateAsync call (ScheduleInspection,
--                  RecordInspectionResult, Retire) since the Domain aggregate always persists via
--                  a full save of its current mutable state. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_Update]
    @NowUtc DATETIME2(3),
    @TransportVehicleId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @VehicleTitle NVARCHAR(200),
    @VehicleType NVARCHAR(50),
    @VehicleIdentifier NVARCHAR(100),
    @LastInspectionAtUtc DATETIME2(3) = NULL,
    @NextInspectionDueAtUtc DATETIME2(3) = NULL,
    @LastInspectionScore DECIMAL(5,2) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[TransportVehicles]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [VehicleTitle] = @VehicleTitle,
            [VehicleType] = @VehicleType,
            [VehicleIdentifier] = @VehicleIdentifier,
            [LastInspectionAtUtc] = @LastInspectionAtUtc,
            [NextInspectionDueAtUtc] = @NextInspectionDueAtUtc,
            [LastInspectionScore] = @LastInspectionScore,
            [ModifiedAtUtc] = @NowUtc
        WHERE [TransportVehicleId] = @TransportVehicleId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

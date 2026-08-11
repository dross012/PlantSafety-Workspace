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
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'TransportVehicles', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'TransportVehicleId=', @TransportVehicleId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TransportVehicleId=', @TransportVehicleId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

    THROW;
END CATCH;
GO

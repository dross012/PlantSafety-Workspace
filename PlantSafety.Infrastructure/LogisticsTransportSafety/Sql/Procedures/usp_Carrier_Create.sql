-- ============================================================================
--  Module:         LogisticsTransportSafety.Carriers
--  File:           usp_Carrier_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[Carriers], backing
--                  ICarrierRepository.AddAsync (RegisterCarrier). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_Create]
    @CarrierId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @CarrierName NVARCHAR(200),
    @CarrierType NVARCHAR(50),
    @InsuranceExpiryAtUtc DATETIME2(3) = NULL,
    @SafetyRatingScore DECIMAL(5,2) = NULL,
    @QualifiedAtUtc DATETIME2(3) = NULL,
    @SuspensionReason NVARCHAR(1000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[Carriers]
            ([CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
             [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@CarrierId, @TenantId, @SiteId, @Status, @CarrierName, @CarrierType, @InsuranceExpiryAtUtc,
             @SafetyRatingScore, @QualifiedAtUtc, @SuspensionReason, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Carriers', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CarrierId=', @CarrierId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CarrierId=', @CarrierId, N'; User=', SYSTEM_USER);
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

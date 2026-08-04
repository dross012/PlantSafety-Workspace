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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ============================================================================
--  Module:         LogisticsTransportSafety.Carriers
--  File:           usp_Carrier_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[Carriers], backing
--                  ICarrierRepository.UpdateAsync. Covers all Carrier lifecycle transitions
--                  driven through the generic UpdateAsync call (QualifyCarrier, SuspendCarrier,
--                  RetireCarrier) since the Domain aggregate always persists via a full save of
--                  its current mutable state. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_Update]
    @NowUtc DATETIME2(3),
    @CarrierId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @CarrierName NVARCHAR(200),
    @CarrierType NVARCHAR(50),
    @InsuranceExpiryAtUtc DATETIME2(3) = NULL,
    @SafetyRatingScore DECIMAL(5,2) = NULL,
    @QualifiedAtUtc DATETIME2(3) = NULL,
    @SuspensionReason NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[Carriers]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [CarrierName] = @CarrierName,
            [CarrierType] = @CarrierType,
            [InsuranceExpiryAtUtc] = @InsuranceExpiryAtUtc,
            [SafetyRatingScore] = @SafetyRatingScore,
            [QualifiedAtUtc] = @QualifiedAtUtc,
            [SuspensionReason] = @SuspensionReason,
            [ModifiedAtUtc] = @NowUtc
        WHERE [CarrierId] = @CarrierId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ============================================================================
--  Module:         LogisticsTransportSafety.DriverQualifications
--  File:           usp_DriverQualification_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[DriverQualifications],
--                  backing IDriverQualificationRepository.UpdateAsync. Covers the Status/
--                  SuspensionReason transitions driven through the generic UpdateAsync call
--                  (SuspendDriverQualification, ReinstateDriverQualification,
--                  RetireDriverQualification) -- matches the exact parameter set sent by
--                  DriverQualificationDapperRepository.UpdateAsync (Status and SuspensionReason
--                  only; other fields are immutable post-creation). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_Update]
    @NowUtc DATETIME2(3),
    @DriverQualificationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @SuspensionReason NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[DriverQualifications]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [SuspensionReason] = @SuspensionReason,
            [ModifiedAtUtc] = @NowUtc
        WHERE [DriverQualificationId] = @DriverQualificationId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

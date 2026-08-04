-- ============================================================================
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  File:           usp_LoadSecurementCheck_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[LoadSecurementChecks],
--                  backing ILoadSecurementCheckRepository.UpdateAsync. Covers
--                  CompleteCorrectiveAction -- matches the exact parameter set sent by
--                  LoadSecurementCheckDapperRepository.UpdateAsync (Status only; check-time
--                  measurements are immutable post-creation). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_Update]
    @NowUtc DATETIME2(3),
    @LoadSecurementCheckId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[LoadSecurementChecks]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc
        WHERE [LoadSecurementCheckId] = @LoadSecurementCheckId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

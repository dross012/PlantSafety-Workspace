-- ============================================================================
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  File:           usp_LoadSecurementCheck_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[LoadSecurementChecks],
--                  backing ILoadSecurementCheckRepository.AddAsync (RecordLoadSecurementCheck).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_Create]
    @LoadSecurementCheckId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @TripId UNIQUEIDENTIFIER = NULL,
    @VehicleId UNIQUEIDENTIFIER,
    @CheckedByUserId UNIQUEIDENTIFIER,
    @CheckedAtUtc DATETIME2(3) = NULL,
    @DeficiencyCount INT,
    @SecurementScore DECIMAL(5,2),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[LoadSecurementChecks]
            ([LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
             [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@LoadSecurementCheckId, @TenantId, @SiteId, @Status, @TripId, @VehicleId, @CheckedByUserId,
             @CheckedAtUtc, @DeficiencyCount, @SecurementScore, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

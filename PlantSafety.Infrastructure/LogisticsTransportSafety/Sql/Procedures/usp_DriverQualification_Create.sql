-- ============================================================================
--  Module:         LogisticsTransportSafety.DriverQualifications
--  File:           usp_DriverQualification_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [LogisticsTransportSafety].[DriverQualifications],
--                  backing IDriverQualificationRepository.AddAsync (QualifyDriver). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_DriverQualification_Create]
    @DriverQualificationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DriverUserId UNIQUEIDENTIFIER,
    @QualificationType NVARCHAR(50),
    @LicenseNumber NVARCHAR(100),
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @SuspensionReason NVARCHAR(1000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [LogisticsTransportSafety].[DriverQualifications]
            ([DriverQualificationId], [TenantId], [SiteId], [Status], [DriverUserId], [QualificationType],
             [LicenseNumber], [IssuedAtUtc], [ExpiresAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy])
        VALUES
            (@DriverQualificationId, @TenantId, @SiteId, @Status, @DriverUserId, @QualificationType,
             @LicenseNumber, @IssuedAtUtc, @ExpiresAtUtc, @SuspensionReason, @CreatedAtUtc, @CreatedBy);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

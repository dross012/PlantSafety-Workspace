-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.ShippingPapers
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatShipment_Update.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PHMSA_HazmatShipment aggregate root
--                  row. Backs IHazmatShipmentRepository.UpdateAsync. @TenantId is part of the
--                  WHERE clause from day one (tenant-scoped write) and @ETag enforces optimistic
--                  concurrency against the native RowVersion column, mirroring
--                  usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT immediately
--                  after the UPDATE (before the audit INSERT, which would otherwise reset it) and
--                  returned via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery always
--                  return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatShipment_Update]
    @HazmatShipmentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @MaterialIds NVARCHAR(MAX) = N'[]',
    @PackagingIds NVARCHAR(MAX) = N'[]',
    @ShippingPaperBasicDescription NVARCHAR(MAX) = NULL,
    @ShippingPaperEmergencyResponseRef NVARCHAR(MAX) = NULL,
    @ShippingPaperCertification NVARCHAR(MAX) = NULL,
    @ShippingPaperWasteManifestReference NVARCHAR(MAX) = NULL,
    @ShippingPaperAdditionalModalRequirements NVARCHAR(MAX) = NULL,
    @ShippingPaperGeneratedAtUtc DATETIME2(3) = NULL,
    @EmergencyResponseErgGuideNumber NVARCHAR(10) = NULL,
    @EmergencyResponseContactName NVARCHAR(200) = NULL,
    @EmergencyResponseContactPhone NVARCHAR(50) = NULL,
    @EmergencyResponseGuidanceSummary NVARCHAR(MAX) = NULL,
    @Mode NVARCHAR(10),
    @PlacardingRequirements NVARCHAR(MAX) = N'[]',
    @SegregationViolations NVARCHAR(MAX) = N'[]',
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Compliance].[PHMSA_HazmatShipment] WHERE HazmatShipmentId = @HazmatShipmentId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'PHMSA_HazmatShipment not found or has been deleted.', 1;

        UPDATE [Compliance].[PHMSA_HazmatShipment]
        SET
            Status = @Status,
            MaterialIds = @MaterialIds,
            PackagingIds = @PackagingIds,
            ShippingPaperBasicDescription = @ShippingPaperBasicDescription,
            ShippingPaperEmergencyResponseRef = @ShippingPaperEmergencyResponseRef,
            ShippingPaperCertification = @ShippingPaperCertification,
            ShippingPaperWasteManifestReference = @ShippingPaperWasteManifestReference,
            ShippingPaperAdditionalModalRequirements = @ShippingPaperAdditionalModalRequirements,
            ShippingPaperGeneratedAtUtc = @ShippingPaperGeneratedAtUtc,
            EmergencyResponseErgGuideNumber = @EmergencyResponseErgGuideNumber,
            EmergencyResponseContactName = @EmergencyResponseContactName,
            EmergencyResponseContactPhone = @EmergencyResponseContactPhone,
            EmergencyResponseGuidanceSummary = @EmergencyResponseGuidanceSummary,
            Mode = @Mode,
            PlacardingRequirements = @PlacardingRequirements,
            SegregationViolations = @SegregationViolations,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE HazmatShipmentId = @HazmatShipmentId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PHMSA_HazmatShipment', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HazmatShipmentId=', @HazmatShipmentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HazmatShipmentId=', @HazmatShipmentId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

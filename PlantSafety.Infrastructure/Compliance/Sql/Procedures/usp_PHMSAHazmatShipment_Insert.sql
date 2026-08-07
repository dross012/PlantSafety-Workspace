-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.ShippingPapers
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatShipment_Insert.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PHMSA_HazmatShipment aggregate root row. Backs
--                  IHazmatShipmentRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging per
--                  the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatShipment_Insert]
    @HazmatShipmentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
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
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[PHMSA_HazmatShipment]
        (
            HazmatShipmentId, TenantId, SiteId, Status, MaterialIds, PackagingIds,
            ShippingPaperBasicDescription, ShippingPaperEmergencyResponseRef, ShippingPaperCertification,
            ShippingPaperWasteManifestReference, ShippingPaperAdditionalModalRequirements, ShippingPaperGeneratedAtUtc,
            EmergencyResponseErgGuideNumber, EmergencyResponseContactName, EmergencyResponseContactPhone, EmergencyResponseGuidanceSummary,
            Mode, PlacardingRequirements, SegregationViolations, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @HazmatShipmentId, @TenantId, @SiteId, @Status, @MaterialIds, @PackagingIds,
            @ShippingPaperBasicDescription, @ShippingPaperEmergencyResponseRef, @ShippingPaperCertification,
            @ShippingPaperWasteManifestReference, @ShippingPaperAdditionalModalRequirements, @ShippingPaperGeneratedAtUtc,
            @EmergencyResponseErgGuideNumber, @EmergencyResponseContactName, @EmergencyResponseContactPhone, @EmergencyResponseGuidanceSummary,
            @Mode, @PlacardingRequirements, @SegregationViolations, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PHMSA_HazmatShipment', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HazmatShipmentId=', @HazmatShipmentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
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

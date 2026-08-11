-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_OtAsset_Insert.sql | Created: 2026-08-04
--  Description: Inserts a new OtAsset row. Backs IOtAssetRepository.AddAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_Insert]
    @OtAssetId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER,
    @AssetTag           NVARCHAR(128),
    @AssetName          NVARCHAR(256),
    @AssetType          NVARCHAR(30),
    @Vendor             NVARCHAR(128),
    @Model              NVARCHAR(128),
    @FirmwareVersion    NVARCHAR(64),
    @NetworkZone        NVARCHAR(64),
    @Criticality        NVARCHAR(20),
    @Status             NVARCHAR(20),
    @LastReconciledAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc       DATETIME2(3),
    @CreatedBy          UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[OtAsset]
        (OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion, NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy)
        VALUES
        (@OtAssetId, @TenantId, @SiteId, @AssetTag, @AssetName, @AssetType, @Vendor, @Model, @FirmwareVersion, @NetworkZone, @Criticality, @Status, @LastReconciledAtUtc, @CreatedAtUtc, @CreatedBy);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'OtAsset', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'OtAssetId=', @OtAssetId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OtAssetId=', @OtAssetId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO

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
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

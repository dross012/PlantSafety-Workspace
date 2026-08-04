-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_OtAsset_GetAllActive.sql | Created: 2026-08-04
--  Description: Retrieves all OtAsset rows for a tenant (in-memory specification filtering
--                applied by OtAssetDapperRepository.ListAsync). Backs the OT Asset Security
--                Status dashboard.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_GetAllActive]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion,
           NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[OtAsset]
    WHERE TenantId = @TenantId
    ORDER BY AssetTag;
END
GO

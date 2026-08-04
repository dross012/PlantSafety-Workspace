-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_OtAsset_GetById.sql | Created: 2026-08-04
--  Description: Retrieves a single OtAsset row by identity, scoped to tenant.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_GetById]
    @OtAssetId UNIQUEIDENTIFIER,
    @TenantId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT OtAssetId, TenantId, SiteId, AssetTag, AssetName, AssetType, Vendor, Model, FirmwareVersion,
           NetworkZone, Criticality, Status, LastReconciledAtUtc, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[OtAsset]
    WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId;
END
GO

-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_OtAsset_Update.sql | Created: 2026-08-04
--  Description: Updates Status/LastReconciledAtUtc on an existing OtAsset row.
--                Backs IOtAssetRepository.UpdateAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_OtAsset_Update]
    @OtAssetId           UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @Status              NVARCHAR(20),
    @LastReconciledAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [CyberPhysicalSecurity].[OtAsset]
        SET Status = @Status,
            LastReconciledAtUtc = @LastReconciledAtUtc
        WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

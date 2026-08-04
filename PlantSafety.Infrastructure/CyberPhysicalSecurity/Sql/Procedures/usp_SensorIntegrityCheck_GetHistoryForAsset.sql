-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SensorIntegrityCheck_GetHistoryForAsset.sql | Created: 2026-08-04
--  Description: Backs GetSensorIntegrityHistoryQuery, most recent first.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetHistoryForAsset]
    @OtAssetId UNIQUEIDENTIFIER,
    @TenantId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
    WHERE OtAssetId = @OtAssetId AND TenantId = @TenantId
    ORDER BY CheckedAtUtc DESC;
END
GO

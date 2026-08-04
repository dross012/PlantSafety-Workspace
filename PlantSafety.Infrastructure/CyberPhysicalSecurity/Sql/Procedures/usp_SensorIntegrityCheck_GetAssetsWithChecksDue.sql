-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SensorIntegrityCheck_GetAssetsWithChecksDue.sql | Created: 2026-08-04
--  Description: Returns the OtAssetId of the most recent check per asset where NextCheckDueUtc
--                has elapsed (recurring sensor integrity check scheduling hook).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetAssetsWithChecksDue]
    @TenantId UNIQUEIDENTIFIER,
    @AsOfUtc  DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    ;WITH LatestPerAsset AS
    (
        SELECT OtAssetId, NextCheckDueUtc,
               ROW_NUMBER() OVER (PARTITION BY OtAssetId ORDER BY CheckedAtUtc DESC) AS rn
        FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
        WHERE TenantId = @TenantId
    )
    SELECT OtAssetId
    FROM LatestPerAsset
    WHERE rn = 1 AND (NextCheckDueUtc IS NULL OR NextCheckDueUtc <= @AsOfUtc);
END
GO

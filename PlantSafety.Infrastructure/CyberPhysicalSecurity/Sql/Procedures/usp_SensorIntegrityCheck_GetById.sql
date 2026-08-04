-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SensorIntegrityCheck_GetById.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_GetById]
    @SensorIntegrityCheckId UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[SensorIntegrityCheck]
    WHERE SensorIntegrityCheckId = @SensorIntegrityCheckId AND TenantId = @TenantId;
END
GO

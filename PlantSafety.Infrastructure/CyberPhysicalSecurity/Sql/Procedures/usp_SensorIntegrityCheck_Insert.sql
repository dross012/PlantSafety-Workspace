-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SensorIntegrityCheck_Insert.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SensorIntegrityCheck_Insert]
    @SensorIntegrityCheckId UNIQUEIDENTIFIER,
    @TenantId               UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER,
    @OtAssetId              UNIQUEIDENTIFIER,
    @CheckType              NVARCHAR(20),
    @Result                 NVARCHAR(20),
    @Details                NVARCHAR(2000),
    @CheckedAtUtc           DATETIME2(3),
    @NextCheckDueUtc        DATETIME2(3) = NULL,
    @CreatedAtUtc           DATETIME2(3),
    @CreatedBy              UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [CyberPhysicalSecurity].[SensorIntegrityCheck]
        (SensorIntegrityCheckId, TenantId, SiteId, OtAssetId, CheckType, Result, Details, CheckedAtUtc, NextCheckDueUtc, CreatedAtUtc, CreatedBy)
        VALUES
        (@SensorIntegrityCheckId, @TenantId, @SiteId, @OtAssetId, @CheckType, @Result, @Details, @CheckedAtUtc, @NextCheckDueUtc, @CreatedAtUtc, @CreatedBy);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

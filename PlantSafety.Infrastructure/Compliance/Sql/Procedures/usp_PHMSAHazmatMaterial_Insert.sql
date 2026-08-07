-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Classification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatMaterial_Insert.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Inserts a new PHMSA_HazmatMaterial aggregate root row. Backs
--                  IHazmatMaterialRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging per
--                  the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatMaterial_Insert]
    @HazmatMaterialId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20),
    @UNNumber NVARCHAR(20),
    @ProperShippingName NVARCHAR(300),
    @HazardClass NVARCHAR(10),
    @PackingGroup NVARCHAR(20),
    @SpecialProvisions NVARCHAR(MAX) = N'[]',
    @ERGGuideNumber NVARCHAR(10) = NULL,
    @IsMarinePollutant BIT = 0,
    @IsTIH BIT = 0,
    @TIHZone NVARCHAR(5) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Compliance].[PHMSA_HazmatMaterial]
        (
            HazmatMaterialId, TenantId, SiteId, Status, UNNumber, ProperShippingName, HazardClass, PackingGroup, SpecialProvisions, ERGGuideNumber, IsMarinePollutant, IsTIH, TIHZone, CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @HazmatMaterialId, @TenantId, @SiteId, @Status, @UNNumber, @ProperShippingName, @HazardClass, @PackingGroup, @SpecialProvisions, @ERGGuideNumber, @IsMarinePollutant, @IsTIH, @TIHZone, @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PHMSA_HazmatMaterial', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'HazmatMaterialId=', @HazmatMaterialId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HazmatMaterialId=', @HazmatMaterialId, N'; User=', SYSTEM_USER);
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

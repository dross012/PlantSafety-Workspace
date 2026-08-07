-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Classification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatMaterial_GetByUNNumber.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Retrieves the single Status='Classified' PHMSA_HazmatMaterial row for a given
--                  (TenantId, UNNumber), if one exists. Mirrors the semantics enforced by the
--                  filtered unique index UX_PHMSA_HazmatMaterial_TenantId_UNNumber_Classified on
--                  [Compliance].[PHMSA_HazmatMaterial] (at most one Classified row per
--                  (TenantId, UNNumber); Draft/Superseded/Retired history rows are excluded here).
--                  Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatMaterial_GetByUNNumber]
    @UNNumber NVARCHAR(20),
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            HazmatMaterialId, TenantId, SiteId, Status, UNNumber, ProperShippingName, HazardClass, PackingGroup,
            SpecialProvisions, ERGGuideNumber, IsMarinePollutant, IsTIH, TIHZone,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Compliance].[PHMSA_HazmatMaterial]
        WHERE UNNumber = @UNNumber AND TenantId = @TenantId AND Status = N'Classified' AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'UNNumber=', @UNNumber, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

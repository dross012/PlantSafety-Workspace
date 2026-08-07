-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Placarding
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAHazmatContainer_GetByMaterialId.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_HazmatContainer rows for a given (TenantId,
--                  MaterialId), ordered by CreatedAtUtc. Supports lookups such as "which container
--                  configurations exist for this classified material" via the
--                  IX_PHMSA_HazmatContainer_MaterialId index. Canonical TRY/CATCH + ErrorDB logging
--                  (no AuditLog for reads).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAHazmatContainer_GetByMaterialId]
    @MaterialId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            HazmatContainerId, TenantId, SiteId, Status, ContainerType, MaterialId, PackagingSpecId,
            Markings, Labels, Placards, OrientationArrowsRequired, CompatibilityGroup,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Compliance].[PHMSA_HazmatContainer]
        WHERE MaterialId = @MaterialId AND TenantId = @TenantId AND IsDeleted = 0
        ORDER BY CreatedAtUtc;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MaterialId=', @MaterialId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

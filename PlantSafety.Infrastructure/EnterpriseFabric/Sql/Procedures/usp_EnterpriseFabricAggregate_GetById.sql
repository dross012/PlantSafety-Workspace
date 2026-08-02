-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnterpriseFabric
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnterpriseFabricAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EnterpriseFabric aggregate root row by identity,
--                  tenant-scoped from day one. Backs IEnterpriseFabricRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetEnterpriseFabricByIdQuery
--                  read path. Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Projects RowVersion as a string ETag. Mirrors
--                  usp_AuditingAggregate_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_EnterpriseFabricAggregate_GetById]
    @EnterpriseFabricId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            EnterpriseFabricId, TenantId, SiteId, Status, FabricArea, SystemReference, IntegrationType,
            OwnerUserId, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [EnterpriseFabric].[EnterpriseFabricAggregate]
        WHERE EnterpriseFabricId = @EnterpriseFabricId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

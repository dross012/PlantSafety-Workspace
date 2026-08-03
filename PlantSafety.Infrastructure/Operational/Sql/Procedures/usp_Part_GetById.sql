-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Part_GetById.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Part aggregate root row by identity, tenant-scoped. Backs
--                  IPartRepository.GetByIdAsync. Projects RowVersion as a string ETag.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_Part_GetById]
    @PartId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            PartId, TenantId, SiteId, PartNumber, PartName, QuantityOnHand, ReorderThreshold, Location,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Operational].[Parts]
        WHERE PartId = @PartId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PartId=', @PartId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

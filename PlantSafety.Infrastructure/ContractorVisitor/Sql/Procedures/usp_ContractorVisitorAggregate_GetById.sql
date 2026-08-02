-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorVisitor
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ContractorVisitorAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single ContractorVisitor aggregate root row by identity,
--                  tenant-scoped from day one. Backs IContractorVisitorRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetContractorVisitorByIdQuery
--                  read path. Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Projects RowVersion as a string ETag.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContractorVisitor].[usp_ContractorVisitorAggregate_GetById]
    @ContractorVisitorId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ContractorVisitorId, TenantId, SiteId, Status, FullName, CompanyName, VisitorType,
            PurposeOfVisit, HostUserId, ScheduledArrivalUtc, ActualArrivalUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [ContractorVisitor].[ContractorVisitorAggregate]
        WHERE ContractorVisitorId = @ContractorVisitorId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteDefinitionAggregate_GetById.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single RouteDefinition aggregate root row by identity,
--                  tenant-scoped from day one. Backs IRouteDefinitionRepository.GetByIdAsync,
--                  GetRouteDefinitionByIdQuery, and GetRouteStepsQuery. Projects RowVersion as a
--                  string ETag. Mirrors usp_ProcessDefinitionAggregate_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteDefinitionAggregate_GetById]
    @RouteDefinitionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            RouteDefinitionId, TenantId, SiteId, PlantName, Name, Status, Version,
            OwnerId, OwnerName, EffectiveFromUtc, EffectiveToUtc, StepsJson,
            CreatedAtUtc, CreatedBy, LastAuditedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [ProcessManagement].[RouteDefinitionAggregate]
        WHERE RouteDefinitionId = @RouteDefinitionId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RouteDefinitionId=', @RouteDefinitionId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

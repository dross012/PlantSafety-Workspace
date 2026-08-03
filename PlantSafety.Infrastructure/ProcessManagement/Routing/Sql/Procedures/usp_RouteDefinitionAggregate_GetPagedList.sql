-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteDefinitionAggregate_GetPagedList.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Returns a single deterministically ordered page of RouteDefinition summary
--                  rows, optionally search-filtered. Backs
--                  IRouteDefinitionReadRepository.GetPagedAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteDefinitionAggregate_GetPagedList]
    @TenantId UNIQUEIDENTIFIER,
    @Search NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            RouteDefinitionId, Name, PlantName, Status, Version, OwnerName, LastAuditedAtUtc
        FROM [ProcessManagement].[RouteDefinitionAggregate]
        WHERE TenantId = @TenantId AND IsDeleted = 0
          AND (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR PlantName LIKE '%' + @Search + '%')
        ORDER BY LastAuditedAtUtc DESC, RouteDefinitionId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteVersionSnapshot_GetLatestForRoute.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Returns the most recently captured RouteVersionSnapshot for a route. Backs
--                  IRouteVersionSnapshotRepository.GetLatestForRouteAsync (used by
--                  ApproveRouteDefinitionCommandHandler to mark the current snapshot Approved).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteVersionSnapshot_GetLatestForRoute]
    @RouteDefinitionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TOP (1)
            RouteVersionId, RouteDefinitionId, TenantId, VersionNumber, Status,
            EffectiveDateUtc, StepsJson, CreatedAtUtc
        FROM [ProcessManagement].[RouteVersionSnapshot]
        WHERE RouteDefinitionId = @RouteDefinitionId AND TenantId = @TenantId
        ORDER BY VersionNumber DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RouteDefinitionId=', @RouteDefinitionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

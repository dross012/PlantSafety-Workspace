-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteVersionSnapshot_GetStepsById.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Returns the captured StepsJson for a single RouteVersionSnapshot, scoped to
--                  the owning route and tenant. Backs
--                  IRouteVersionSnapshotReadRepository.GetStepSnapshotsAsync (the Diff Viewer's
--                  two-version comparison).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteVersionSnapshot_GetStepsById]
    @RouteVersionId UNIQUEIDENTIFIER,
    @RouteDefinitionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT StepsJson
        FROM [ProcessManagement].[RouteVersionSnapshot]
        WHERE RouteVersionId = @RouteVersionId AND RouteDefinitionId = @RouteDefinitionId AND TenantId = @TenantId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RouteVersionId=', @RouteVersionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

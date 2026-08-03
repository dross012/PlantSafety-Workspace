-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteDefinitionAggregate_Insert.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Inserts a new RouteDefinition aggregate root row. Backs
--                  IRouteDefinitionRepository.AddAsync. Mirrors
--                  usp_ProcessDefinitionAggregate_Insert.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteDefinitionAggregate_Insert]
    @RouteDefinitionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @PlantName NVARCHAR(200),
    @Name NVARCHAR(200),
    @Status NVARCHAR(50),
    @Version INT,
    @OwnerId UNIQUEIDENTIFIER,
    @OwnerName NVARCHAR(200),
    @EffectiveFromUtc DATETIME2(3),
    @EffectiveToUtc DATETIME2(3) = NULL,
    @StepsJson NVARCHAR(MAX),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER,
    @LastAuditedAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ProcessManagement].[RouteDefinitionAggregate]
        (
            RouteDefinitionId, TenantId, SiteId, PlantName, Name, Status, Version,
            OwnerId, OwnerName, EffectiveFromUtc, EffectiveToUtc, StepsJson,
            CreatedAtUtc, CreatedBy, LastAuditedAtUtc, IsDeleted
        )
        VALUES
        (
            @RouteDefinitionId, @TenantId, @SiteId, @PlantName, @Name, @Status, @Version,
            @OwnerId, @OwnerName, @EffectiveFromUtc, @EffectiveToUtc, @StepsJson,
            @CreatedAtUtc, @CreatedBy, @LastAuditedAtUtc, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RouteDefinitionAggregate', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RouteDefinitionId=', @RouteDefinitionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RouteDefinitionId=', @RouteDefinitionId, N'; User=', SYSTEM_USER);
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

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement.Routing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RouteDefinitionAggregate_Update.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Overwrites the mutable lifecycle/structural state of a RouteDefinition
--                  aggregate root row. Backs IRouteDefinitionRepository.UpdateAsync. @ETag
--                  enforces optimistic concurrency against the native RowVersion column.
--                  @RowsAffected is captured via @@ROWCOUNT and returned via SELECT -- callers
--                  MUST invoke this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync).
--                  Mirrors usp_ProcessDefinitionAggregate_Update.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_RouteDefinitionAggregate_Update]
    @RouteDefinitionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Version INT,
    @StepsJson NVARCHAR(MAX),
    @LastAuditedAtUtc DATETIME2(3),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [ProcessManagement].[RouteDefinitionAggregate] WHERE RouteDefinitionId = @RouteDefinitionId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'RouteDefinition not found or has been deleted.', 1;

        UPDATE [ProcessManagement].[RouteDefinitionAggregate]
        SET
            Status = @Status,
            Version = @Version,
            StepsJson = @StepsJson,
            LastAuditedAtUtc = @LastAuditedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE RouteDefinitionId = @RouteDefinitionId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RouteDefinitionAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RouteDefinitionId=', @RouteDefinitionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
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

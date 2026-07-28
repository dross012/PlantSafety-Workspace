-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Commands
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperationalResources_BulkUpdate.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IOperationalBulkRepository.BulkUpdateAsync.
--                  Invoked once per resource id by the caller so that individual item
--                  failures can be reported without aborting the remainder of the batch.
--                  The interface does not surface @CorrelationId/@ActorId, so only
--                  @NowUtc is threaded through as an audit timestamp.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_OperationalResources_BulkUpdate]
    @TenantId         UNIQUEIDENTIFIER,
    @SiteId           UNIQUEIDENTIFIER = NULL,
    @ResourceId       UNIQUEIDENTIFIER,
    @Operation        NVARCHAR(50),
    @AssignedToUserId UNIQUEIDENTIFIER = NULL,
    @Notes            NVARCHAR(MAX) = NULL,
    @NowUtc           DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Operational].[OperationalResources]
        SET
            Status = CASE @Operation
                        WHEN 'Close'    THEN 'Closed'
                        WHEN 'Approve'  THEN 'Approved'
                        WHEN 'Reject'   THEN 'Rejected'
                        WHEN 'Reopen'   THEN 'Open'
                        ELSE Status
                     END,
            AssignedToUserId = CASE WHEN @Operation = 'Reassign' THEN @AssignedToUserId ELSE AssignedToUserId END,
            Notes = COALESCE(@Notes, Notes),
            UpdatedAtUtc = @NowUtc
        WHERE Id = @ResourceId
          AND TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    BEGIN TRY
        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'OperationalResources', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    -- SET NOCOUNT ON means SqlCommand.ExecuteNonQuery (DapperCommandExecutor.ExecuteAsync) always
    -- returns -1, not the true affected-row count -- OperationalBulkDapperRepository.BulkUpdateAsync's
    -- "rowsAffected > 0" not-found check could therefore never distinguish a real update from a
    -- miss, silently reporting NOT_FOUND for every successful bulk update. Callers must switch to
    -- DapperCommandExecutor.ExecuteScalarAsync to read this explicit rowcount.
    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResourceId=', @ResourceId, N'; TenantId=', @TenantId, N'; Operation=', @Operation, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
GO

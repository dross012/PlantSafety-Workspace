-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_SafetyEventsResources_BulkUpdate.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing ISafetyEventsBulkRepository.BulkUpdateAsync.
--                  Invoked once per resource id by the caller so that individual item
--                  failures can be reported without aborting the remainder of the batch.
--                  The interface does not surface @CorrelationId/@ActorId, so only
--                  @NowUtc is threaded through as an audit timestamp.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_SafetyEventsResources_BulkUpdate]
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

        UPDATE [SafetyEvents].[SafetyEventsResources]
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

    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'SafetyEventsResources', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ResourceId=', @ResourceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ResourceId=', @ResourceId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

    THROW;
END CATCH;
GO

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

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

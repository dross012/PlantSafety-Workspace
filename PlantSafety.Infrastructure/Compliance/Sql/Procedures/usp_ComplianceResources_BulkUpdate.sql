-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_ComplianceResources_BulkUpdate.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IComplianceBulkRepository.BulkUpdateAsync.
--                  Invoked once per resource id by the caller so that individual item
--                  failures can be reported without aborting the remainder of the batch.
--                  The interface does not surface @CorrelationId/@ActorId, so only
--                  @NowUtc is threaded through as an audit timestamp.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceResources_BulkUpdate]
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

        UPDATE [Compliance].[ComplianceResources]
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

    -- SET NOCOUNT ON means SqlCommand.ExecuteNonQuery (DapperCommandExecutor.ExecuteAsync) always
    -- returns -1, not the true affected-row count -- ComplianceBulkDapperRepository.BulkUpdateAsync's
    -- "rowsAffected > 0" not-found check could therefore never distinguish a real update from a
    -- miss, silently reporting NOT_FOUND for every successful bulk update. Callers must switch to
    -- DapperCommandExecutor.ExecuteScalarAsync to read this explicit rowcount.
    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

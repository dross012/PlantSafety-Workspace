-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletionItemResult_Upsert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Idempotent insert-if-not-exists for
--                  [Checklists].[ChecklistCompletionItemResult]. Called once per in-memory
--                  item result for every UpdateAsync so that newly-recorded results (added
--                  via ChecklistCompletion.RecordItemResult()/FlagFailedItem()) are persisted
--                  without a separate Application-layer repository call.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletionItemResult_Upsert]
    @TenantId                 UNIQUEIDENTIFIER,
    @CorrelationId            UNIQUEIDENTIFIER,
    @ActorId                  UNIQUEIDENTIFIER,
    @NowUtc                   DATETIME2(3),
    @ItemResultId             UNIQUEIDENTIFIER,
    @ChecklistCompletionId    UNIQUEIDENTIFIER,
    @ChecklistTemplateItemId  UNIQUEIDENTIFIER,
    @Passed                   BIT,
    @IsCriticalFailure        BIT,
    @Notes                    NVARCHAR(2000) = NULL,
    @RecordedAtUtc            DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM [Checklists].[ChecklistCompletionItemResult] WHERE [ItemResultId] = @ItemResultId)
        BEGIN
            INSERT INTO [Checklists].[ChecklistCompletionItemResult]
                ([ItemResultId], [ChecklistCompletionId], [ChecklistTemplateItemId], [Passed],
                 [IsCriticalFailure], [Notes], [RecordedAtUtc])
            VALUES
                (@ItemResultId, @ChecklistCompletionId, @ChecklistTemplateItemId, @Passed,
                 @IsCriticalFailure, @Notes, @RecordedAtUtc);
        END
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ChecklistCompletionItemResult', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ItemResultId=', @ItemResultId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ItemResultId=', @ItemResultId, N'; User=', SYSTEM_USER);
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

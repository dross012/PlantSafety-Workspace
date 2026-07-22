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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

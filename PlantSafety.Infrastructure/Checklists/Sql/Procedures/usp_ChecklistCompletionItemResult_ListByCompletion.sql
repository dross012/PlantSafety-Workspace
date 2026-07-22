-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletionItemResult_ListByCompletion.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure returning all ChecklistCompletionItemResult rows for a
--                  given ChecklistCompletionId, used to rehydrate the
--                  ChecklistCompletion.ItemResults collection. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletionItemResult_ListByCompletion]
    @ChecklistCompletionId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [ItemResultId], [ChecklistCompletionId], [ChecklistTemplateItemId], [Passed],
    [IsCriticalFailure], [Notes], [RecordedAtUtc]
FROM [Checklists].[ChecklistCompletionItemResult]
WHERE [ChecklistCompletionId] = @ChecklistCompletionId
ORDER BY [RecordedAtUtc] ASC;
GO

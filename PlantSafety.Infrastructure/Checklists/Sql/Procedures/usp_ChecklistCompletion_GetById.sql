-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletion_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IChecklistCompletionRepository.GetByIdAsync (header
--                  row only; item results are fetched separately via
--                  usp_ChecklistCompletionItemResult_ListByCompletion and merged in the
--                  repository). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletion_GetById]
    @ChecklistCompletionId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [ChecklistCompletionId], [TenantId], [SiteId], [Status], [ChecklistTemplateId],
    [ItemsTotal], [ItemsPassed], [ItemsFailed], [Outcome],
    [CreatedAtUtc], [CreatedBy], [StartedAtUtc], [CompletedAtUtc], [CriticalFailureCount]
FROM [Checklists].[ChecklistCompletion]
WHERE [ChecklistCompletionId] = @ChecklistCompletionId
  AND ISNULL([IsDeleted], 0) = 0;
GO

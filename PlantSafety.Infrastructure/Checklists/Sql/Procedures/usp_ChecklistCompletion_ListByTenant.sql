-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletion_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IChecklistCompletionRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; ChecklistCompletionSpecification
--                  predicates are applied in-memory in the repository after full aggregate
--                  rehydration. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletion_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [ChecklistCompletionId], [TenantId], [SiteId], [Status], [ChecklistTemplateId],
    [ItemsTotal], [ItemsPassed], [ItemsFailed], [Outcome],
    [CreatedAtUtc], [CreatedBy], [StartedAtUtc], [CompletedAtUtc], [CriticalFailureCount]
FROM [Checklists].[ChecklistCompletion]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC;
GO

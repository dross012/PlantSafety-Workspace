-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplate_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IChecklistTemplateRepository.GetByIdAsync (header
--                  row only; items are fetched separately via
--                  usp_ChecklistTemplateItem_ListByTemplate and merged in the repository).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplate_GetById]
    @ChecklistTemplateId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [ChecklistTemplateId], [TenantId], [SiteId], [Status], [TemplateTitle], [Category],
    [Description], [ItemCount], [IsMandatory], [RegulatoryReference],
    [CreatedAtUtc], [CreatedBy], [PublishedAtUtc], [RetiredAtUtc], [RetirementReason]
FROM [Checklists].[ChecklistTemplate]
WHERE [ChecklistTemplateId] = @ChecklistTemplateId
  AND ISNULL([IsDeleted], 0) = 0;
GO

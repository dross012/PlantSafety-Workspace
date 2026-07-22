-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplateItem_ListByTemplate.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure returning all ChecklistTemplateItem rows for a given
--                  ChecklistTemplateId, in sequence order, used to rehydrate the
--                  ChecklistTemplate.Items collection. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplateItem_ListByTemplate]
    @ChecklistTemplateId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [ItemId], [ChecklistTemplateId], [Sequence], [Description], [IsCritical],
    [RegulatoryReference], [AddedAtUtc]
FROM [Checklists].[ChecklistTemplateItem]
WHERE [ChecklistTemplateId] = @ChecklistTemplateId
ORDER BY [Sequence] ASC;
GO

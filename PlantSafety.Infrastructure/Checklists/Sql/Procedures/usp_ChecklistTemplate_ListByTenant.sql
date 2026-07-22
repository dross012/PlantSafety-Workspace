-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplate_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IChecklistTemplateRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; ChecklistTemplateSpecification
--                  predicates are applied in-memory in the repository after full aggregate
--                  rehydration (Dapper has no LINQ/Expression SQL translator).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplate_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [ChecklistTemplateId], [TenantId], [SiteId], [Status], [TemplateTitle], [Category],
    [Description], [ItemCount], [IsMandatory], [RegulatoryReference],
    [CreatedAtUtc], [CreatedBy], [PublishedAtUtc], [RetiredAtUtc], [RetirementReason]
FROM [Checklists].[ChecklistTemplate]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC;
GO

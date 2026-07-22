-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencySystemsAndLifeSafety_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IEmergencySystemsAndLifeSafetyRepository.GetItemsAsync. Thin,
--                  category-scoped projection over the shared
--                  [Checklists].[ChecklistCategoryRecord] table, filtered to the
--                  'EmergencySystemsAndLifeSafety' category (Emergency Systems & Life Safety Checklists).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_EmergencySystemsAndLifeSafety_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
SELECT
    [ItemId], [ChecklistTemplateId], [TenantId], [SiteId], [Title], [Status], [RecordedAtUtc]
FROM [Checklists].[ChecklistCategoryRecord]
WHERE [Category]  = N'EmergencySystemsAndLifeSafety'
  AND [TenantId]   = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [RecordedAtUtc] DESC;
GO

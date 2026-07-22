-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists (category read-models)
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistCategoryRecord.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Shared physical read-model table backing the 8 category-scoped read
--                  repositories under ChecklistTemplate/<Category>/Repositories
--                  (ConfinedSpaceEntry, ContractorSafety, EmergencySystemsAndLifeSafety,
--                  HazardousMaterialsAndChemicalSafety, HotWork, MachineGuardingAndLoto,
--                  PersonalProtectiveEquipment, WorkingAtHeights). Each category's read
--                  repository queries this one table filtered by its own [Category]
--                  discriminator value via a dedicated, thin usp_<Category>_GetItems
--                  stored procedure -- see project_plantsafety_domains.md / ChecklistCategory
--                  smart enum for the canonical category code list.
--
--                  ASSUMPTION (flagged per "no-guessing" rule): the 8 sibling *ItemDto
--                  read-models are identically shaped (ItemId, ChecklistTemplateId,
--                  TenantId, SiteId, Title, Status, RecordedAtUtc) and are documented as
--                  "projected from" the ChecklistTemplate aggregate, but there is no
--                  distinct child-entity source in the Domain layer for them (only
--                  ChecklistTemplateItem, which has a different shape: Description/
--                  IsCritical, no Status/Title). The most complete, standards-compliant
--                  interpretation is a dedicated per-category projection table, populated
--                  by the same write path that maintains ChecklistTemplate/ChecklistTemplateItem
--                  (future population job/trigger is out of scope for this Infrastructure
--                  pass, which focuses on making every repository interface compile and
--                  query correctly against real DDL).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistCategoryRecord' AND schema_id = SCHEMA_ID('Checklists'))
BEGIN
    CREATE TABLE [Checklists].[ChecklistCategoryRecord] (
        [ItemId]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ChecklistTemplateId] UNIQUEIDENTIFIER NOT NULL,
        [TenantId]            UNIQUEIDENTIFIER NOT NULL,
        [SiteId]              UNIQUEIDENTIFIER NULL,
        [Category]            NVARCHAR(100)    NOT NULL,
        [Title]               NVARCHAR(200)    NOT NULL,
        [Status]              NVARCHAR(20)     NOT NULL,
        [RecordedAtUtc]       DATETIME2(3)     NOT NULL,
        [IsDeleted]           BIT              NOT NULL CONSTRAINT DF_ChecklistCategoryRecord_IsDeleted DEFAULT (0),
        CONSTRAINT FK_ChecklistCategoryRecord_ChecklistTemplate
            FOREIGN KEY ([ChecklistTemplateId]) REFERENCES [Checklists].[ChecklistTemplate] ([ChecklistTemplateId])
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistCategoryRecord_Category_TenantId_RecordedAtUtc]
        ON [Checklists].[ChecklistCategoryRecord] ([Category], [TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [Title], [Status], [ChecklistTemplateId]);
END
GO

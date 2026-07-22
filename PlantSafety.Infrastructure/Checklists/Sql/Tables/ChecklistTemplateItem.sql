-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistTemplateItem.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ChecklistTemplateItem child entity, owned exclusively
--                  by the ChecklistTemplate aggregate root. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistTemplateItem' AND schema_id = SCHEMA_ID('Checklists'))
BEGIN
    CREATE TABLE [Checklists].[ChecklistTemplateItem] (
        [ItemId]              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ChecklistTemplateId] UNIQUEIDENTIFIER NOT NULL,
        [Sequence]            INT              NOT NULL,
        [Description]         NVARCHAR(1000)   NOT NULL,
        [IsCritical]          BIT              NOT NULL CONSTRAINT DF_ChecklistTemplateItem_IsCritical DEFAULT (0),
        [RegulatoryReference] NVARCHAR(200)    NULL,
        [AddedAtUtc]          DATETIME2(3)     NOT NULL,
        CONSTRAINT FK_ChecklistTemplateItem_ChecklistTemplate
            FOREIGN KEY ([ChecklistTemplateId]) REFERENCES [Checklists].[ChecklistTemplate] ([ChecklistTemplateId])
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistTemplateItem_ChecklistTemplateId]
        ON [Checklists].[ChecklistTemplateItem] ([ChecklistTemplateId], [Sequence]);
END
GO

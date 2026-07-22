-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistCompletionItemResult.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ChecklistCompletionItemResult child entity, owned
--                  exclusively by the ChecklistCompletion aggregate root. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistCompletionItemResult' AND schema_id = SCHEMA_ID('Checklists'))
BEGIN
    CREATE TABLE [Checklists].[ChecklistCompletionItemResult] (
        [ItemResultId]            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ChecklistCompletionId]   UNIQUEIDENTIFIER NOT NULL,
        [ChecklistTemplateItemId] UNIQUEIDENTIFIER NOT NULL,
        [Passed]                  BIT              NOT NULL,
        [IsCriticalFailure]       BIT              NOT NULL CONSTRAINT DF_ChecklistCompletionItemResult_IsCriticalFailure DEFAULT (0),
        [Notes]                   NVARCHAR(2000)   NULL,
        [RecordedAtUtc]           DATETIME2(3)     NOT NULL,
        CONSTRAINT FK_ChecklistCompletionItemResult_ChecklistCompletion
            FOREIGN KEY ([ChecklistCompletionId]) REFERENCES [Checklists].[ChecklistCompletion] ([ChecklistCompletionId])
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistCompletionItemResult_ChecklistCompletionId]
        ON [Checklists].[ChecklistCompletionItemResult] ([ChecklistCompletionId]);
END
GO

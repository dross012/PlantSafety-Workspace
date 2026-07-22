-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistCompletion.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ChecklistCompletion aggregate root backing
--                  IChecklistCompletionRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy
--                  in 90-row-level-security.sql. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistCompletion' AND schema_id = SCHEMA_ID('Checklists'))
BEGIN
    CREATE TABLE [Checklists].[ChecklistCompletion] (
        [ChecklistCompletionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER NULL,
        [Status]                NVARCHAR(20)     NOT NULL CONSTRAINT DF_ChecklistCompletion_Status DEFAULT ('Created'),
        [ChecklistTemplateId]   UNIQUEIDENTIFIER NOT NULL,
        [ItemsTotal]            INT              NOT NULL CONSTRAINT DF_ChecklistCompletion_ItemsTotal DEFAULT (0),
        [ItemsPassed]           INT              NOT NULL CONSTRAINT DF_ChecklistCompletion_ItemsPassed DEFAULT (0),
        [ItemsFailed]           INT              NOT NULL CONSTRAINT DF_ChecklistCompletion_ItemsFailed DEFAULT (0),
        [Outcome]               NVARCHAR(30)     NOT NULL CONSTRAINT DF_ChecklistCompletion_Outcome DEFAULT ('Pending'),
        [CreatedAtUtc]          DATETIME2(3)     NOT NULL,
        [CreatedBy]             UNIQUEIDENTIFIER NOT NULL,
        [StartedAtUtc]          DATETIME2(3)     NULL,
        [CompletedAtUtc]        DATETIME2(3)     NULL,
        [CriticalFailureCount]  INT              NOT NULL CONSTRAINT DF_ChecklistCompletion_CriticalFailureCount DEFAULT (0),
        [IsDeleted]             BIT              NOT NULL CONSTRAINT DF_ChecklistCompletion_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]     DATETIME2(3)     NOT NULL CONSTRAINT DF_ChecklistCompletion_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistCompletion_TenantId_SiteId]
        ON [Checklists].[ChecklistCompletion] ([TenantId], [SiteId])
        INCLUDE ([Status], [ChecklistTemplateId], [Outcome]);
END
GO

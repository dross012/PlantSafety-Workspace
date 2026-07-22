-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Checklists
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistsReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ChecklistsReadModel read-model aggregate backing
--                  IChecklistsRepository.GetChecklistsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistsReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ChecklistsReadModel] (
        [ChecklistId]     UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Name]                    NVARCHAR(400)       NOT NULL,
        [Category]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [CompletedAtUtc]  DATETIME2(3)        NOT NULL,
        [CompletedBy]     UNIQUEIDENTIFIER    NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistsReadModel_TenantId_CompletedAtUtc]
        ON [Operational].[ChecklistsReadModel] ([TenantId], [CompletedAtUtc])
        INCLUDE ([SiteId]);
END
GO

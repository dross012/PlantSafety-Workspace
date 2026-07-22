-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           ChecklistTemplate.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ChecklistTemplate aggregate root backing
--                  IChecklistTemplateRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy
--                  in 90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Checklists')
BEGIN
    EXEC('CREATE SCHEMA [Checklists]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ChecklistTemplate' AND schema_id = SCHEMA_ID('Checklists'))
BEGIN
    CREATE TABLE [Checklists].[ChecklistTemplate] (
        [ChecklistTemplateId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]            UNIQUEIDENTIFIER NOT NULL,
        [SiteId]              UNIQUEIDENTIFIER NULL,
        [Status]              NVARCHAR(20)     NOT NULL CONSTRAINT DF_ChecklistTemplate_Status DEFAULT ('Created'),
        [TemplateTitle]       NVARCHAR(200)    NOT NULL,
        [Category]            NVARCHAR(100)    NOT NULL,
        [Description]         NVARCHAR(2000)   NOT NULL CONSTRAINT DF_ChecklistTemplate_Description DEFAULT (''),
        [ItemCount]           INT              NOT NULL CONSTRAINT DF_ChecklistTemplate_ItemCount DEFAULT (0),
        [IsMandatory]         BIT              NOT NULL CONSTRAINT DF_ChecklistTemplate_IsMandatory DEFAULT (0),
        [RegulatoryReference] NVARCHAR(200)    NULL,
        [CreatedAtUtc]        DATETIME2(3)     NOT NULL,
        [CreatedBy]           UNIQUEIDENTIFIER NOT NULL,
        [PublishedAtUtc]      DATETIME2(3)     NULL,
        [RetiredAtUtc]        DATETIME2(3)     NULL,
        [RetirementReason]    NVARCHAR(500)    NULL,
        [IsDeleted]           BIT              NOT NULL CONSTRAINT DF_ChecklistTemplate_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]   DATETIME2(3)     NOT NULL CONSTRAINT DF_ChecklistTemplate_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ChecklistTemplate_TenantId_SiteId]
        ON [Checklists].[ChecklistTemplate] ([TenantId], [SiteId])
        INCLUDE ([Status], [Category], [TemplateTitle]);
END
GO

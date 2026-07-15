-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           Processes.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProcessDefinition aggregate backing IProcessRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ProcessManagement')
BEGIN
    EXEC('CREATE SCHEMA [ProcessManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Processes' AND schema_id = SCHEMA_ID('ProcessManagement'))
BEGIN
    CREATE TABLE [ProcessManagement].[Processes] (
        [ProcessId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ProcessName]       NVARCHAR(200)       NOT NULL,
        [Category]          NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL CONSTRAINT DF_Processes_Status DEFAULT ('Draft'),
        [Version]           INT                 NOT NULL CONSTRAINT DF_Processes_Version DEFAULT (1),
        [OwnerId]           UNIQUEIDENTIFIER    NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
        [CreatedAt]         DATETIME2(3)        NOT NULL,
        [LastModifiedAtUtc] DATETIME2(3)        NOT NULL CONSTRAINT DF_Processes_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Processes_TenantId_ProcessName]
        ON [ProcessManagement].[Processes] ([TenantId], [ProcessName])
        INCLUDE ([SiteId], [Category], [Status]);
END
GO

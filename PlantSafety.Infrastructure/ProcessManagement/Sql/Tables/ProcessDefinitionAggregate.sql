-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ProcessDefinitionAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the ProcessDefinition aggregate root (IProcessDefinitionRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "ProcessDefinitionAggregate" (not bare "ProcessDefinition") to avoid any
--                  ambiguity with the pre-existing, unrelated Features-vertical
--                  [ProcessManagement].[Processes] table that lives under this same module
--                  folder (backs the generic "Process" tracking Features slice, a different
--                  concept). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native
--                  ROWVERSION concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ProcessManagement')
    EXEC('CREATE SCHEMA ProcessManagement AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProcessDefinitionAggregate' AND schema_id = SCHEMA_ID('ProcessManagement'))
BEGIN
    CREATE TABLE [ProcessManagement].[ProcessDefinitionAggregate]
    (
        ProcessDefinitionId   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProcessDefinitionAggregate PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        ProcessName           NVARCHAR(200)    NOT NULL,
        Category              NVARCHAR(100)    NOT NULL,
        Status                NVARCHAR(50)     NOT NULL CONSTRAINT DF_ProcessDefinitionAggregate_Status DEFAULT (N'Draft'),
        Version               INT              NOT NULL CONSTRAINT DF_ProcessDefinitionAggregate_Version DEFAULT (1),
        OwnerId               UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_ProcessDefinitionAggregate_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ProcessDefinitionAggregate_TenantId_Site ON [ProcessManagement].[ProcessDefinitionAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ProcessDefinitionAggregate_Status ON [ProcessManagement].[ProcessDefinitionAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ProcessDefinitionAggregate_CreatedAtUtc ON [ProcessManagement].[ProcessDefinitionAggregate] (CreatedAtUtc);
END
GO

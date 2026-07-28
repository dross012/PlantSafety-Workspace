-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.MOCWorkflows
--  Layer:          Infrastructure – SQL Scripts
--  File:           MOCWorkflows.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ManagementOfChange.MOCWorkflows read-model, backing
--                  IMOCWorkflowsRepository.GetMOCWorkflowsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCWorkflows' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCWorkflows] (
        WorkflowId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        ChangeTitle   NVARCHAR(300)    NOT NULL,
        CurrentStage  NVARCHAR(100)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        InitiatedAtUtc DATETIME2(3)    NOT NULL,
        InitiatedBy   UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCWorkflows_TenantId_SiteId] ON [Compliance].[MOCWorkflows] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCWorkflows_InitiatedAtUtc] ON [Compliance].[MOCWorkflows] (InitiatedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[MOCWorkflows]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[MOCWorkflows] ADD RowVersion ROWVERSION NOT NULL;
END
GO

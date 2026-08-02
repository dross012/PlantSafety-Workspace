-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PermitTemplate.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PermitTemplate aggregate root (IPermitTemplateRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
--  UPDATED:        2026-07-29 -- Added native RowVersion ROWVERSION column (Gap 2, Phase 4
--                  PermitToWork concurrency remediation). See PermitWorkflow.sql's matching
--                  remark for full rationale (ConcurrencyToken was regenerated but never
--                  checked; RowVersion + string ETag mirrors the SafetyEvents.Incident /
--                  ProcessManagement.Processes pattern). ConcurrencyToken retained, additive-only.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PermitToWork')
    EXEC('CREATE SCHEMA PermitToWork AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitTemplate' AND schema_id = SCHEMA_ID('PermitToWork'))
BEGIN
    CREATE TABLE [PermitToWork].[PermitTemplate]
    (
        PermitTemplateId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PermitTemplate PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PermitTypeCode NVARCHAR(400) NOT NULL,
        RequiredControls NVARCHAR(400) NOT NULL,
        StandardValidityHours INT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PermitTemplate_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermitTemplate_Concurrency DEFAULT (NEWID()),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PermitTemplate_TenantId_Site ON [PermitToWork].[PermitTemplate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitTemplate_Status ON [PermitToWork].[PermitTemplate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitTemplate_CreatedAtUtc ON [PermitToWork].[PermitTemplate] (CreatedAtUtc);
END
GO

-- Schema-evolution guard for environments where [PermitToWork].[PermitTemplate] was already
-- deployed without RowVersion (ROWVERSION cannot be added retroactively via the CREATE
-- TABLE-only block above, which only executes on first deploy).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitTemplate' AND schema_id = SCHEMA_ID('PermitToWork'))
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[PermitToWork].[PermitTemplate]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [PermitToWork].[PermitTemplate] ADD RowVersion ROWVERSION NOT NULL;
END
GO

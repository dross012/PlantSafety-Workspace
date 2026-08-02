-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PermitExpiration.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PermitExpiration aggregate root (IPermitExpirationRepository).
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitExpiration' AND schema_id = SCHEMA_ID('PermitToWork'))
BEGIN
    CREATE TABLE [PermitToWork].[PermitExpiration]
    (
        PermitExpirationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PermitExpiration PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PermitNumber NVARCHAR(400) NOT NULL,
        IssuedAtUtc DATETIME2(3) NULL,
        ExpiresAtUtc DATETIME2(3) NULL,
        ExtensionRequested BIT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PermitExpiration_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermitExpiration_Concurrency DEFAULT (NEWID()),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PermitExpiration_TenantId_Site ON [PermitToWork].[PermitExpiration] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitExpiration_Status ON [PermitToWork].[PermitExpiration] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitExpiration_CreatedAtUtc ON [PermitToWork].[PermitExpiration] (CreatedAtUtc);
END
GO

-- Schema-evolution guard for environments where [PermitToWork].[PermitExpiration] was already
-- deployed without RowVersion (ROWVERSION cannot be added retroactively via the CREATE
-- TABLE-only block above, which only executes on first deploy).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitExpiration' AND schema_id = SCHEMA_ID('PermitToWork'))
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[PermitToWork].[PermitExpiration]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [PermitToWork].[PermitExpiration] ADD RowVersion ROWVERSION NOT NULL;
END
GO

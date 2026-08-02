-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PermitWorkflow.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PermitWorkflow aggregate root (IPermitWorkflowRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
--  UPDATED:        2026-07-29 -- Added native RowVersion ROWVERSION column (Gap 2, Phase 4
--                  PermitToWork concurrency remediation). The pre-existing ConcurrencyToken
--                  UNIQUEIDENTIFIER column was regenerated via NEWID() on every UPDATE but never
--                  checked anywhere -- no optimistic-concurrency enforcement existed (blind
--                  overwrite). Replaced with the same native ROWVERSION + string-ETag pattern
--                  already used by SafetyEvents.Incident / ProcessManagement.Processes
--                  (see usp_PermitWorkflow_Update.sql / usp_PermitWorkflow_GetById.sql for the
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) ETag convention).
--                  ROWVERSION cannot be added inside the original CREATE TABLE-only idempotent
--                  block below (that block only runs once, when the table does not yet exist),
--                  so a second idempotent ALTER TABLE guard is appended for already-deployed
--                  environments. ConcurrencyToken is left in place (additive-only; not read by
--                  any new code path) rather than dropped, per the "do not remove until
--                  re-verified unreferenced" deletion discipline.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PermitToWork')
    EXEC('CREATE SCHEMA PermitToWork AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitWorkflow' AND schema_id = SCHEMA_ID('PermitToWork'))
BEGIN
    CREATE TABLE [PermitToWork].[PermitWorkflow]
    (
        PermitWorkflowId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PermitWorkflow PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PermitNumber NVARCHAR(400) NOT NULL,
        PermitTypeCode NVARCHAR(400) NOT NULL,
        RequesterUserId UNIQUEIDENTIFIER NULL,
        IssuerUserId UNIQUEIDENTIFIER NULL,
        WorkDescription NVARCHAR(400) NOT NULL,
        RequiredControls NVARCHAR(400) NULL,
        RiskScore INT NULL,
        ControlsConfirmedBy NVARCHAR(400) NULL,
        ControlsImplementedAtUtc DATETIME2(3) NULL,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc DATETIME2(3) NULL,
        IssuedAtUtc DATETIME2(3) NULL,
        ExpiresAtUtc DATETIME2(3) NULL,
        ClosureSignedBy UNIQUEIDENTIFIER NULL,
        ClosureNotes NVARCHAR(400) NULL,
        ClosedAtUtc DATETIME2(3) NULL,
        ArchivedAtUtc DATETIME2(3) NULL,
        RejectedReason NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PermitWorkflow_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermitWorkflow_Concurrency DEFAULT (NEWID()),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PermitWorkflow_TenantId_Site ON [PermitToWork].[PermitWorkflow] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitWorkflow_Status ON [PermitToWork].[PermitWorkflow] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitWorkflow_CreatedAtUtc ON [PermitToWork].[PermitWorkflow] (CreatedAtUtc);
END
GO

-- Schema-evolution guard for environments where [PermitToWork].[PermitWorkflow] was already
-- deployed without RowVersion (ROWVERSION cannot be added retroactively via the CREATE
-- TABLE-only block above, which only executes on first deploy).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitWorkflow' AND schema_id = SCHEMA_ID('PermitToWork'))
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[PermitToWork].[PermitWorkflow]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [PermitToWork].[PermitWorkflow] ADD RowVersion ROWVERSION NOT NULL;
END
GO

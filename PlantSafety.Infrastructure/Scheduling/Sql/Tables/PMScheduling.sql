-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PMScheduling.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PMScheduling aggregate root (IPMSchedulingRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
--  UPDATED:        2026-07-30 (Phase 4 Scheduling.PMScheduling concurrency remediation) -- added a
--                  native RowVersion ROWVERSION NOT NULL column, additive alongside the pre-existing
--                  ConcurrencyToken UNIQUEIDENTIFIER DEFAULT (NEWID()) column, matching every other
--                  Phase 1-3 module (see RiskManagement's Risk.sql for the identical precedent).
--                  Exposed to callers as ETag via
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) in
--                  usp_PMScheduling_GetById. Idempotent via COL_LENGTH guard; safe to re-run
--                  against a database that already has the column.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PMScheduling' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[PMScheduling]
    (
        PMSchedulingId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PMScheduling PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        EquipmentId UNIQUEIDENTIFIER NOT NULL,
        EquipmentDescription NVARCHAR(400) NOT NULL,
        PmTaskType NVARCHAR(400) NOT NULL,
        DueAtUtc DATETIME2(3) NULL,
        IsOverdue BIT NOT NULL,
        EscalationPriorityCode NVARCHAR(400) NULL,
        CompletionNotes NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PMScheduling_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PMScheduling_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PMScheduling_TenantId_Site ON [Scheduling].[PMScheduling] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PMScheduling_Status ON [Scheduling].[PMScheduling] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PMScheduling_CreatedAtUtc ON [Scheduling].[PMScheduling] (CreatedAtUtc);
END
GO

-- Additive: native ROWVERSION concurrency column, alongside the pre-existing (vestigial)
-- ConcurrencyToken UNIQUEIDENTIFIER column -- see header comment. Idempotent.
IF COL_LENGTH('[Scheduling].[PMScheduling]', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [Scheduling].[PMScheduling] ADD RowVersion ROWVERSION NOT NULL;
END
GO

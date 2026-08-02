-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           EmergencyDrillPlanning.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the EmergencyDrillPlanning aggregate root (IEmergencyDrillPlanningRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
--  UPDATED:        2026-07-30 -- Phase 4 full build-out: added a native RowVersion ROWVERSION
--                  column (idempotent ALTER TABLE guard) to back optimistic-concurrency ETags on
--                  usp_EmergencyDrillPlanning_Update/GetById, mirroring [Compliance].[Permits].
--                  RowVersion is additive alongside the pre-existing ConcurrencyToken column
--                  (left untouched/unused rather than removed, since it may still be referenced
--                  elsewhere) -- RowVersion is the column actually projected as ETag going forward.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyDrillPlanning' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[EmergencyDrillPlanning]
    (
        EmergencyDrillPlanningId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmergencyDrillPlanning PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        DrillType NVARCHAR(400) NOT NULL,
        ShiftCode NVARCHAR(400) NOT NULL,
        ScheduledAtUtc DATETIME2(3) NULL,
        ParticipationCount INT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_EmergencyDrillPlanning_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyDrillPlanning_Concurrency DEFAULT (NEWID()),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_TenantId_Site ON [Scheduling].[EmergencyDrillPlanning] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_Status ON [Scheduling].[EmergencyDrillPlanning] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_CreatedAtUtc ON [Scheduling].[EmergencyDrillPlanning] (CreatedAtUtc);
END
GO

-- Idempotent guard: adds RowVersion to a pre-existing table created before this migration pass
-- (a table cannot have two ROWVERSION/TIMESTAMP columns, so this only ever fires once).
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmergencyDrillPlanning' AND schema_id = SCHEMA_ID('Scheduling'))
   AND NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('[Scheduling].[EmergencyDrillPlanning]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Scheduling].[EmergencyDrillPlanning] ADD RowVersion ROWVERSION NOT NULL;
END
GO

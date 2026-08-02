-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ShiftFatigue.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the ShiftFatigue aggregate root (IShiftFatigueRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ShiftFatigue' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[ShiftFatigue]
    (
        ShiftFatigueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShiftFatigue PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        ShiftPattern NVARCHAR(400) NOT NULL,
        ConsecutiveNightShifts INT NOT NULL,
        OvertimeHours DECIMAL(18,4) NOT NULL,
        FatigueRiskScore DECIMAL(18,4) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_ShiftFatigue_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ShiftFatigue_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_ShiftFatigue_TenantId_Site ON [Scheduling].[ShiftFatigue] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ShiftFatigue_Status ON [Scheduling].[ShiftFatigue] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ShiftFatigue_CreatedAtUtc ON [Scheduling].[ShiftFatigue] (CreatedAtUtc);
END
GO

-- Phase 4 full build-out (2026-07-30): native ROWVERSION concurrency column, mirroring
-- [Compliance].[Permits]'s convention. Idempotent guard so this is safe to re-run against a
-- table that was created before this column existed. The pre-existing ConcurrencyToken
-- UNIQUEIDENTIFIER column is left in place (additive-only evolution) but is superseded by
-- RowVersion for the @ETag concurrency check in usp_ShiftFatigue_Update.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Scheduling].[ShiftFatigue]') AND name = 'RowVersion')
    ALTER TABLE [Scheduling].[ShiftFatigue] ADD RowVersion ROWVERSION NOT NULL;
GO

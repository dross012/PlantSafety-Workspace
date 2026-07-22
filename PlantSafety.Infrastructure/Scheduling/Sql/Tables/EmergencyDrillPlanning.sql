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
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EmergencyDrillPlanning_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_TenantId_Site ON [Scheduling].[EmergencyDrillPlanning] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_Status ON [Scheduling].[EmergencyDrillPlanning] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EmergencyDrillPlanning_CreatedAtUtc ON [Scheduling].[EmergencyDrillPlanning] (CreatedAtUtc);
END
GO

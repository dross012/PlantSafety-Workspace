-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PermitScheduling.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PermitScheduling aggregate root (IPermitSchedulingRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PermitToWork')
    EXEC('CREATE SCHEMA PermitToWork AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitScheduling' AND schema_id = SCHEMA_ID('PermitToWork'))
BEGIN
    CREATE TABLE [PermitToWork].[PermitScheduling]
    (
        PermitSchedulingId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PermitScheduling PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PermitNumber NVARCHAR(400) NOT NULL,
        ScheduledStartUtc DATETIME2(3) NULL,
        ScheduledEndUtc DATETIME2(3) NULL,
        ConflictDetected BIT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PermitScheduling_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermitScheduling_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PermitScheduling_TenantId_Site ON [PermitToWork].[PermitScheduling] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitScheduling_Status ON [PermitToWork].[PermitScheduling] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitScheduling_CreatedAtUtc ON [PermitToWork].[PermitScheduling] (CreatedAtUtc);
END
GO

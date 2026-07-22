-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.MachineGuarding
--  Layer:          Infrastructure - SQL Scripts
--  File:           MachineGuarding.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the MachineGuarding aggregate root backing IMachineGuardingRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EmployeeSafety')
BEGIN
    EXEC('CREATE SCHEMA [EmployeeSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MachineGuarding' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[MachineGuarding] (
        [MachineGuardingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [GuardTitle] NVARCHAR(400) NOT NULL,
        [GuardType] NVARCHAR(100) NOT NULL,
        [EquipmentTag] NVARCHAR(400) NOT NULL,
        [Location] NVARCHAR(400) NOT NULL,
        [HasInterlock] BIT NOT NULL CONSTRAINT DF_MachineGuarding_HasInterlock DEFAULT (0),
        [InspectionIntervalDays] INT NOT NULL CONSTRAINT DF_MachineGuarding_InspectionIntervalDays DEFAULT (0),
        [LastInspectedByUserId] UNIQUEIDENTIFIER NULL,
        [LastInspectedAtUtc] DATETIME2(3) NULL,
        [NextInspectionDueAtUtc] DATETIME2(3) NULL,
        [OpenDeficiencyCount] INT NOT NULL CONSTRAINT DF_MachineGuarding_OpenDeficiencyCount DEFAULT (0),
        [GuardConditionScore] DECIMAL(9,2) NULL,
        [BypassDetected] BIT NOT NULL CONSTRAINT DF_MachineGuarding_BypassDetected DEFAULT (0),
        [LastFindingNotes] NVARCHAR(2000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_MachineGuarding_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MachineGuarding_TenantId_SiteId]
        ON [EmployeeSafety].[MachineGuarding] ([TenantId], [SiteId])
        INCLUDE ([Status], [GuardTitle]);
END
GO

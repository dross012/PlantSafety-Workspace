-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.MachineGuardsBarriers
--  Layer:          Infrastructure - SQL Scripts
--  File:           MachineGuardsBarriers.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the MachineGuardsBarriers aggregate root backing IMachineGuardsBarriersRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'MachineSafety')
BEGIN
    EXEC('CREATE SCHEMA [MachineSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MachineGuardsBarriers' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[MachineGuardsBarriers] (
        [MachineGuardsBarriersId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [GuardTitle] NVARCHAR(400) NOT NULL,
        [GuardType] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [InstalledByUserId] UNIQUEIDENTIFIER NOT NULL,
        [InspectedAtUtc] DATETIME2(3) NULL,
        [NextInspectionDueAtUtc] DATETIME2(3) NULL,
        [DeficiencyCount] INT NOT NULL CONSTRAINT DF_MachineGuardsBarriers_DeficiencyCount DEFAULT (0),
        [GuardIntegrityScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_MachineGuardsBarriers_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MachineGuardsBarriers_TenantId_SiteId]
        ON [MachineSafety].[MachineGuardsBarriers] ([TenantId], [SiteId])
        INCLUDE ([Status], [GuardTitle]);
END
GO

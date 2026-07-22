-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MachineGuardingInspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           MachineGuardingInspections.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the MachineGuardingInspections aggregate root backing IMachineGuardingInspectionsRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyProcedures')
BEGIN
    EXEC('CREATE SCHEMA [SafetyProcedures]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MachineGuardingInspections' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[MachineGuardingInspections] (
        [MachineGuardingInspectionsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [InspectionTitle] NVARCHAR(400) NOT NULL,
        [InspectionCategory] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [InspectedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [InspectedAtUtc] DATETIME2(3) NULL,
        [NextInspectionDueAtUtc] DATETIME2(3) NULL,
        [DeficiencyCount] INT NOT NULL CONSTRAINT DF_MachineGuardingInspections_DeficiencyCount DEFAULT (0),
        [ComplianceScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_MachineGuardingInspections_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MachineGuardingInspections_TenantId_SiteId]
        ON [SafetyProcedures].[MachineGuardingInspections] ([TenantId], [SiteId])
        INCLUDE ([Status], [InspectionTitle]);
END
GO

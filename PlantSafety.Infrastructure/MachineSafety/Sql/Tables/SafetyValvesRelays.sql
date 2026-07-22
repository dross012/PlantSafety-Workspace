-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.SafetyValvesRelays
--  Layer:          Infrastructure - SQL Scripts
--  File:           SafetyValvesRelays.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyValvesRelays aggregate root backing ISafetyValvesRelaysRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SafetyValvesRelays' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[SafetyValvesRelays] (
        [SafetyValvesRelaysId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ComponentTitle] NVARCHAR(400) NOT NULL,
        [ComponentType] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [InspectedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [InspectedAtUtc] DATETIME2(3) NULL,
        [NextInspectionDueAtUtc] DATETIME2(3) NULL,
        [FaultCount] INT NOT NULL CONSTRAINT DF_SafetyValvesRelays_FaultCount DEFAULT (0),
        [ReliabilityScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_SafetyValvesRelays_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_SafetyValvesRelays_TenantId_SiteId]
        ON [MachineSafety].[SafetyValvesRelays] ([TenantId], [SiteId])
        INCLUDE ([Status], [ComponentTitle]);
END
GO

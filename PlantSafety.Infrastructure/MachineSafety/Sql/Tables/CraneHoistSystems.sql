-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           CraneHoistSystems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CraneHoistSystems aggregate root backing
--                  ICraneHoistSystemsRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'MachineSafety')
BEGIN
    EXEC('CREATE SCHEMA [MachineSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CraneHoistSystems' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[CraneHoistSystems] (
        [CraneHoistSystemsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AssetTag] NVARCHAR(400) NOT NULL,
        [AssetTitle] NVARCHAR(400) NOT NULL,
        [AssetType] NVARCHAR(100) NOT NULL,
        [RatedLoadCapacityKg] DECIMAL(12,2) NOT NULL,
        [LastObservedLoadKg] DECIMAL(12,2) NULL,
        [LastLiftAtUtc] DATETIME2(3) NULL,
        [OverloadEventCount] INT NOT NULL CONSTRAINT DF_CraneHoistSystems_OverloadEventCount DEFAULT (0),
        [OverloadFlagged] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_OverloadFlagged DEFAULT (0),
        [InspectionDueAtUtc] DATETIME2(3) NULL,
        [LastInspectionAtUtc] DATETIME2(3) NULL,
        [LastInspectionPassed] BIT NULL,
        [OperatorUserId] UNIQUEIDENTIFIER NOT NULL,
        [OperatorQualifiedAtUtc] DATETIME2(3) NULL,
        [OperatorQualificationExpiresAtUtc] DATETIME2(3) NULL,
        [OperatorQualificationSuspended] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_OperatorQualificationSuspended DEFAULT (0),
        [LastRiggingInspectionAtUtc] DATETIME2(3) NULL,
        [LastRiggingDefectDetected] BIT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CraneHoistSystems_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_TenantId_SiteId]
        ON [MachineSafety].[CraneHoistSystems] ([TenantId], [SiteId])
        INCLUDE ([Status], [AssetTitle]);

    CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_InspectionDueAtUtc]
        ON [MachineSafety].[CraneHoistSystems] ([TenantId], [InspectionDueAtUtc])
        INCLUDE ([SiteId], [Status]);

    CREATE NONCLUSTERED INDEX [IX_CraneHoistSystems_OperatorQualificationExpiresAtUtc]
        ON [MachineSafety].[CraneHoistSystems] ([TenantId], [OperatorQualificationExpiresAtUtc])
        INCLUDE ([SiteId], [OperatorUserId]);
END
GO

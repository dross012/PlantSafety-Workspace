-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           LotoSystems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LotoSystems aggregate root backing ILotoSystemsRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LotoSystems' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[LotoSystems] (
        [LotoSystemsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [SystemTitle] NVARCHAR(400) NOT NULL,
        [EquipmentTag] NVARCHAR(400) NOT NULL,
        [EnergySourceTypes] NVARCHAR(100) NOT NULL,
        [IsolationPointDescription] NVARCHAR(2000) NOT NULL,
        [IsolationPointCount] INT NOT NULL CONSTRAINT DF_LotoSystems_IsolationPointCount DEFAULT (0),
        [VerifiedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [VerifiedZeroEnergyAtUtc] DATETIME2(3) NULL,
        [AuditedAtUtc] DATETIME2(3) NULL,
        [ComplianceScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_LotoSystems_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LotoSystems_TenantId_SiteId]
        ON [MachineSafety].[LotoSystems] ([TenantId], [SiteId])
        INCLUDE ([Status], [SystemTitle]);
END
GO

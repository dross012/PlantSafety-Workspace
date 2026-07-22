-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           TwoHandControls.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TwoHandControls aggregate root backing ITwoHandControlsRepository.
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TwoHandControls' AND schema_id = SCHEMA_ID('MachineSafety'))
BEGIN
    CREATE TABLE [MachineSafety].[TwoHandControls] (
        [TwoHandControlsId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ControlTitle] NVARCHAR(400) NOT NULL,
        [EquipmentType] NVARCHAR(100) NOT NULL,
        [EquipmentLocation] NVARCHAR(400) NOT NULL,
        [TestedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [TestedAtUtc] DATETIME2(3) NULL,
        [NextTestDueAtUtc] DATETIME2(3) NULL,
        [BypassAttemptCount] INT NOT NULL CONSTRAINT DF_TwoHandControls_BypassAttemptCount DEFAULT (0),
        [SynchronizationScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_TwoHandControls_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TwoHandControls_TenantId_SiteId]
        ON [MachineSafety].[TwoHandControls] ([TenantId], [SiteId])
        INCLUDE ([Status], [ControlTitle]);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.LockoutTagout
--  Layer:          Infrastructure - SQL Scripts
--  File:           LockoutTagoutReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the LockoutTagoutReadModel read-model aggregate backing
--                  ILockoutTagoutRepository.GetLockoutTagoutAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LockoutTagoutReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[LockoutTagoutReadModel] (
        [LockoutTagoutId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [LockType]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [LockedAtUtc]     DATETIME2(3)        NOT NULL,
        [LockedBy]        UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_LockoutTagoutReadModel_TenantId_LockedAtUtc]
        ON [Operational].[LockoutTagoutReadModel] ([TenantId], [LockedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[LockoutTagoutReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[LockoutTagoutReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

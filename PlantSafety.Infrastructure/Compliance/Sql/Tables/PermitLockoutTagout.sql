-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.LockoutTagout
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitLockoutTagout.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.LockoutTagout read-model, backing
--                  ILockoutTagoutRepository.GetLockoutTagoutAsync. Named with the
--                  Permit prefix to avoid collision with the unrelated
--                  Compliance.Procedures.LockoutTagoutProcedure aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLockoutTagout' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLockoutTagout] (
        PermitId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        EquipmentIdentifier NVARCHAR(200)    NOT NULL,
        LockHolderName      NVARCHAR(200)    NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        IssuedAtUtc         DATETIME2(3)     NOT NULL,
        ExpiresAtUtc        DATETIME2(3)     NOT NULL,
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLockoutTagout_TenantId_SiteId] ON [Compliance].[PermitLockoutTagout] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLockoutTagout_IssuedAtUtc] ON [Compliance].[PermitLockoutTagout] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitLockoutTagout]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitLockoutTagout] ADD RowVersion ROWVERSION NOT NULL;
END
GO

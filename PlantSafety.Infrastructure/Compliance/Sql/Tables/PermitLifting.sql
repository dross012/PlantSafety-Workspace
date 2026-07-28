-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.Lifting
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitLifting.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.Lifting read-model, backing
--                  ILiftingRepository.GetLiftingAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLifting' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLifting] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        LoadDescription NVARCHAR(300)    NOT NULL,
        LoadWeightKg    DECIMAL(12,2)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLifting_TenantId_SiteId] ON [Compliance].[PermitLifting] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLifting_IssuedAtUtc] ON [Compliance].[PermitLifting] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitLifting]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitLifting] ADD RowVersion ROWVERSION NOT NULL;
END
GO

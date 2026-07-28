-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.PressureTesting
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitPressureTesting.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.PressureTesting read-model, backing
--                  IPressureTestingRepository.GetPressureTestingAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitPressureTesting' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitPressureTesting] (
        PermitId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        TestArticle      NVARCHAR(200)    NOT NULL,
        TestPressurePsi  DECIMAL(12,2)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitPressureTesting_TenantId_SiteId] ON [Compliance].[PermitPressureTesting] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitPressureTesting_IssuedAtUtc] ON [Compliance].[PermitPressureTesting] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitPressureTesting]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitPressureTesting] ADD RowVersion ROWVERSION NOT NULL;
END
GO

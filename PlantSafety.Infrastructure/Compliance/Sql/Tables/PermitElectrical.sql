-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.Electrical
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitElectrical.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.Electrical read-model, backing
--                  IElectricalRepository.GetElectricalAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitElectrical' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitElectrical] (
        PermitId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        EquipmentIdentifier  NVARCHAR(200)    NOT NULL,
        VoltageClass         NVARCHAR(50)     NOT NULL,
        Status               NVARCHAR(50)     NOT NULL,
        IssuedAtUtc          DATETIME2(3)     NOT NULL,
        ExpiresAtUtc         DATETIME2(3)     NOT NULL,
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitElectrical_TenantId_SiteId] ON [Compliance].[PermitElectrical] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitElectrical_IssuedAtUtc] ON [Compliance].[PermitElectrical] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitElectrical]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitElectrical] ADD RowVersion ROWVERSION NOT NULL;
END
GO

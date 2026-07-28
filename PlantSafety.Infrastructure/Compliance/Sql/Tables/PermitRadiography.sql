-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.Radiography
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitRadiography.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.Radiography read-model, backing
--                  IRadiographyRepository.GetRadiographyAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitRadiography' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitRadiography] (
        PermitId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        SourceIsotope        NVARCHAR(100)    NOT NULL,
        ExclusionZoneMeters  DECIMAL(10,2)    NOT NULL,
        Status               NVARCHAR(50)     NOT NULL,
        IssuedAtUtc          DATETIME2(3)     NOT NULL,
        ExpiresAtUtc         DATETIME2(3)     NOT NULL,
        ApprovedBy           UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitRadiography_TenantId_SiteId] ON [Compliance].[PermitRadiography] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitRadiography_IssuedAtUtc] ON [Compliance].[PermitRadiography] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitRadiography]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitRadiography] ADD RowVersion ROWVERSION NOT NULL;
END
GO

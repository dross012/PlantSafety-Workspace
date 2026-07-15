-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.Excavation
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitExcavation.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.Excavation read-model, backing
--                  IExcavationRepository.GetExcavationAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitExcavation' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitExcavation] (
        PermitId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ExcavationLocation NVARCHAR(200)   NOT NULL,
        DepthMeters       DECIMAL(10,2)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitExcavation_TenantId_SiteId] ON [Compliance].[PermitExcavation] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitExcavation_IssuedAtUtc] ON [Compliance].[PermitExcavation] (IssuedAtUtc);
END
GO

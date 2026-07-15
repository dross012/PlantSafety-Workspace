-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.ConfinedSpace
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitConfinedSpace.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.ConfinedSpace read-model, backing
--                  IConfinedSpaceRepository.GetConfinedSpaceAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitConfinedSpace' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitConfinedSpace] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        SpaceIdentifier NVARCHAR(200)    NOT NULL,
        AttendantName   NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitConfinedSpace_TenantId_SiteId] ON [Compliance].[PermitConfinedSpace] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitConfinedSpace_IssuedAtUtc] ON [Compliance].[PermitConfinedSpace] (IssuedAtUtc);
END
GO

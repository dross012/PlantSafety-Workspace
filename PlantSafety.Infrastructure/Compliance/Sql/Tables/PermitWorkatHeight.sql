-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.WorkatHeight
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitWorkatHeight.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.WorkatHeight read-model, backing
--                  IWorkatHeightRepository.GetWorkatHeightAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitWorkatHeight' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitWorkatHeight] (
        PermitId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        WorkLocation        NVARCHAR(200)    NOT NULL,
        FallProtectionType  NVARCHAR(100)    NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        IssuedAtUtc         DATETIME2(3)     NOT NULL,
        ExpiresAtUtc        DATETIME2(3)     NOT NULL,
        ApprovedBy          UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitWorkatHeight_TenantId_SiteId] ON [Compliance].[PermitWorkatHeight] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitWorkatHeight_IssuedAtUtc] ON [Compliance].[PermitWorkatHeight] (IssuedAtUtc);
END
GO

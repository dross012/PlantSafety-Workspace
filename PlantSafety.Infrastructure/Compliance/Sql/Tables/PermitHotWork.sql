-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.HotWork
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitHotWork.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.HotWork read-model, backing
--                  IHotWorkRepository.GetHotWorkAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitHotWork' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitHotWork] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        WorkDescription NVARCHAR(500)    NOT NULL,
        FireWatchName   NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitHotWork_TenantId_SiteId] ON [Compliance].[PermitHotWork] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitHotWork_IssuedAtUtc] ON [Compliance].[PermitHotWork] (IssuedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.ColdWork
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitColdWork.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.ColdWork read-model, backing
--                  IColdWorkRepository.GetColdWorkAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitColdWork' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitColdWork] (
        PermitId        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        WorkDescription NVARCHAR(500)    NOT NULL,
        Location        NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitColdWork_TenantId_SiteId] ON [Compliance].[PermitColdWork] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitColdWork_IssuedAtUtc] ON [Compliance].[PermitColdWork] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PermitColdWork]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PermitColdWork] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.HotTap
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitHotTap.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.HotTap read-model, backing
--                  IHotTapRepository.GetHotTapAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitHotTap' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitHotTap] (
        PermitId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        LineIdentifier    NVARCHAR(200)    NOT NULL,
        PipelineContents  NVARCHAR(200)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        ApprovedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitHotTap_TenantId_SiteId] ON [Compliance].[PermitHotTap] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitHotTap_IssuedAtUtc] ON [Compliance].[PermitHotTap] (IssuedAtUtc);
END
GO

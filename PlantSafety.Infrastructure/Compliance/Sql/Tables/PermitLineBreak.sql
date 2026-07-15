-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.LineBreak
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitLineBreak.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.LineBreak read-model, backing
--                  ILineBreakRepository.GetLineBreakAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitLineBreak' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitLineBreak] (
        PermitId         UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        LineIdentifier   NVARCHAR(200)    NOT NULL,
        ProcessContents  NVARCHAR(200)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        ApprovedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitLineBreak_TenantId_SiteId] ON [Compliance].[PermitLineBreak] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitLineBreak_IssuedAtUtc] ON [Compliance].[PermitLineBreak] (IssuedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.AuditFindings
--  Layer:          Infrastructure – SQL Scripts
--  File:           AuditFindings.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AuditFindings read-model, backing
--                  IAuditFindingsRepository.GetFindingsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFindings' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[AuditFindings] (
        FindingId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AuditingId      UNIQUEIDENTIFIER NOT NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Title           NVARCHAR(200)    NOT NULL,
        Severity        NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IdentifiedAtUtc DATETIME2(3)     NOT NULL,
        IdentifiedBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFindings_TenantId_SiteId] ON [Compliance].[AuditFindings] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AuditFindings_IdentifiedAtUtc] ON [Compliance].[AuditFindings] (IdentifiedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_AuditFindings_AuditingId] ON [Compliance].[AuditFindings] (AuditingId);
END
GO

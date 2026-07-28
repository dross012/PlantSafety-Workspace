-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.ComplianceAudit
--  Layer:          Infrastructure – SQL Scripts
--  File:           ComplianceAudit.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ComplianceAudit read-model, backing
--                  IComplianceAuditRepository.GetAuditsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceAudit] (
        AuditingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        AuditType       NVARCHAR(100)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ConductedAtUtc  DATETIME2(3)     NOT NULL,
        ConductedBy     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ComplianceAudit_TenantId_SiteId] ON [Compliance].[ComplianceAudit] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ComplianceAudit_ConductedAtUtc] ON [Compliance].[ComplianceAudit] (ConductedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[ComplianceAudit]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[ComplianceAudit] ADD RowVersion ROWVERSION NOT NULL;
END
GO

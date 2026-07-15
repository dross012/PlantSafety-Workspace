-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.InternalSafetyAudit
--  Layer:          Infrastructure – SQL Scripts
--  File:           InternalSafetyAudit.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InternalSafetyAudit read-model, backing
--                  IInternalSafetyAuditRepository.GetAuditsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InternalSafetyAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InternalSafetyAudit] (
        AuditingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Scope           NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ConductedAtUtc  DATETIME2(3)     NOT NULL,
        ConductedBy     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InternalSafetyAudit_TenantId_SiteId] ON [Compliance].[InternalSafetyAudit] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_InternalSafetyAudit_ConductedAtUtc] ON [Compliance].[InternalSafetyAudit] (ConductedAtUtc);
END
GO

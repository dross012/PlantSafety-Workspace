-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.DeadlinesAnnualAudit
--  Layer:          Infrastructure - SQL Scripts
--  File:           DeadlinesAnnualAudit.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DeadlinesAnnualAudit read-model aggregate backing
--                  IAnnualAuditRepository.GetDeadlinesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeadlinesAnnualAudit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[DeadlinesAnnualAudit] (
        [DeadlineId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [AuditScope]              NVARCHAR(300)       NOT NULL,
        [DueAtUtc]                DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DeadlinesAnnualAudit_TenantId_DueAtUtc]
        ON [Compliance].[DeadlinesAnnualAudit] ([TenantId], [DueAtUtc])
        INCLUDE ([SiteId]);
END
GO
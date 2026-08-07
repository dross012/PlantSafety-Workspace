-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding.NfpaCoverage
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditFindingNfpaCoverage.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many link between an AuditFinding and the specific
--                  Compliance.NFPA_HazardProfile row(s) that finding's evidence actually covers.
--                  This -- not a coarse module-name tag on AuditFinding -- is what makes real,
--                  per-hazard NFPA gap detection possible: "guidance X exists for module Y and
--                  has never been linked from any finding at site Z" is only answerable once
--                  individual coverage links exist. AppliesToModule for a finding is never
--                  stored directly; it's always derived transitively through the linked
--                  hazards' own module mapping, so there is exactly one source of truth for
--                  module classification (Compliance.NFPA_HazardModuleMapping), not two
--                  parallel taxonomies that could drift apart. Mirrors
--                  Auditing.AuditFindingOshaCoverage.
--                  No TenantId column -- tenancy is inherited via the AuditFindingId FK to
--                  [Auditing].[AuditFinding], matching this codebase's existing child-entity
--                  convention (e.g. AuditFindingOshaCoverage, ChecklistTemplateItem,
--                  AgencyContact).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFindingNfpaCoverage' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditFindingNfpaCoverage] (
        [AuditFindingNfpaCoverageId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AuditFindingNfpaCoverage_AuditFindingNfpaCoverageId DEFAULT (NEWID()) CONSTRAINT PK_AuditFindingNfpaCoverage PRIMARY KEY,
        [AuditFindingId]             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingNfpaCoverage_AuditFinding REFERENCES [Auditing].[AuditFinding] ([AuditFindingId]),
        [HazardId]                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingNfpaCoverage_NFPA_HazardProfile REFERENCES [Compliance].[NFPA_HazardProfile] ([HazardId]),
        [CreatedAtUtc]               DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditFindingNfpaCoverage_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [CreatedBy]                  NVARCHAR(200)    NULL,
        CONSTRAINT UX_AuditFindingNfpaCoverage_Finding_Hazard UNIQUE ([AuditFindingId], [HazardId])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFindingNfpaCoverage_HazardId] ON [Auditing].[AuditFindingNfpaCoverage] ([HazardId]);
    CREATE NONCLUSTERED INDEX [IX_AuditFindingNfpaCoverage_AuditFindingId] ON [Auditing].[AuditFindingNfpaCoverage] ([AuditFindingId]);
END
GO

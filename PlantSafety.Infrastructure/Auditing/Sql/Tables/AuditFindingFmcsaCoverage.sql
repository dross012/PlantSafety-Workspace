-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding.FmcsaCoverage
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditFindingFmcsaCoverage.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many link between an AuditFinding and the specific
--                  Compliance.FMCSARequirement row(s) that finding's evidence actually
--                  covers. This -- not a coarse module-name tag on AuditFinding -- is what
--                  makes real, per-requirement FMCSA gap detection possible: "requirement X
--                  exists for module Y and has never been linked from any finding at site Z"
--                  is only answerable once individual coverage links exist. AppliesToModule
--                  for a finding is never stored directly; it's always derived transitively
--                  through the linked requirements' own AppliesToModule, so there is exactly
--                  one source of truth for module classification
--                  (Compliance.FMCSARequirementModuleMapping), not two parallel taxonomies
--                  that could drift apart. Mirrors Auditing.AuditFindingOshaCoverage.
--                  No TenantId column -- tenancy is inherited via the AuditFindingId FK to
--                  [Auditing].[AuditFinding], matching this codebase's existing child-entity
--                  convention (e.g. ChecklistTemplateItem, AgencyContact).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFindingFmcsaCoverage' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditFindingFmcsaCoverage] (
        [AuditFindingFmcsaCoverageId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditFindingFmcsaCoverage PRIMARY KEY CONSTRAINT DF_AuditFindingFmcsaCoverage_AuditFindingFmcsaCoverageId DEFAULT (NEWID()),
        [AuditFindingId]              UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingFmcsaCoverage_AuditFinding REFERENCES [Auditing].[AuditFinding] ([AuditFindingId]),
        [RequirementId]               UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingFmcsaCoverage_FMCSARequirement REFERENCES [Compliance].[FMCSARequirement] ([RequirementId]),
        [CreatedAtUtc]                DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditFindingFmcsaCoverage_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [CreatedBy]                   UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT UX_AuditFindingFmcsaCoverage_Finding_Requirement UNIQUE ([AuditFindingId], [RequirementId])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFindingFmcsaCoverage_RequirementId] ON [Auditing].[AuditFindingFmcsaCoverage] ([RequirementId]);
    CREATE NONCLUSTERED INDEX [IX_AuditFindingFmcsaCoverage_AuditFindingId] ON [Auditing].[AuditFindingFmcsaCoverage] ([AuditFindingId]);
END
GO

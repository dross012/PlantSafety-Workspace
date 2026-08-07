-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding.OshaCoverage
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditFindingOshaCoverage.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many link between an AuditFinding and the specific
--                  Compliance.OSHARequirement row(s) that finding's evidence actually covers.
--                  This -- not a coarse module-name tag on AuditFinding -- is what makes real,
--                  per-requirement OSHA gap detection possible: "requirement X exists for module
--                  Y and has never been linked from any finding at site Z" is only answerable
--                  once individual coverage links exist. AppliesToModule for a finding is never
--                  stored directly; it's always derived transitively through the linked
--                  requirements' own AppliesToModule, so there is exactly one source of truth
--                  for module classification (Compliance.OSHARequirementModuleMapping), not two
--                  parallel taxonomies that could drift apart.
--                  No TenantId column -- tenancy is inherited via the AuditFindingId FK to
--                  [Auditing].[AuditFinding], matching this codebase's existing child-entity
--                  convention (e.g. ChecklistTemplateItem, AgencyContact).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFindingOshaCoverage' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditFindingOshaCoverage] (
        [AuditFindingOshaCoverageId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditFindingOshaCoverage PRIMARY KEY,
        [AuditFindingId]             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingOshaCoverage_AuditFinding REFERENCES [Auditing].[AuditFinding] ([AuditFindingId]),
        [OSHARequirementId]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_AuditFindingOshaCoverage_OSHARequirement REFERENCES [Compliance].[OSHARequirement] ([OSHARequirementId]),
        [CreatedAtUtc]               DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditFindingOshaCoverage_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [CreatedBy]                  UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT UX_AuditFindingOshaCoverage_Finding_Requirement UNIQUE ([AuditFindingId], [OSHARequirementId])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFindingOshaCoverage_OSHARequirementId] ON [Auditing].[AuditFindingOshaCoverage] ([OSHARequirementId]);
    CREATE NONCLUSTERED INDEX [IX_AuditFindingOshaCoverage_AuditFindingId] ON [Auditing].[AuditFindingOshaCoverage] ([AuditFindingId]);
END
GO

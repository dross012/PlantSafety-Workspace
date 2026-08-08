-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure - SQL Scripts
--  File:           StandardsRequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Individual ANSI/ASME/ISO consensus-standard requirement rows keyed to a
--                  StandardsFamily/StandardsSeries. Global regulatory-adjacent reference
--                  data mirroring the OSHAStandard/OSHARequirement and EpaRegulatoryRequirement
--                  precedent, not tenant business data (no TenantId, no soft-delete).
--                  ImpactedModules is a free-text, comma-delimited list of PlantSafety
--                  module names (matching the OSHARequirement.AppliesToModule /
--                  EpaRegulatoryRequirement.ImpactedModules convention) rather than a DB
--                  enum, since a requirement can affect more than one module and the module
--                  set evolves independently of this reference table. Severity uses the
--                  exact codes of the shared PlantSafety.Domain.Compliance.RulesEngine.
--                  Enums.ComplianceSeverity smart enum (Advisory, Low, Medium, High,
--                  Critical). SeriesCode is deliberately left as an informational
--                  NVARCHAR(30) rather than a strict FK to the (FamilyCode, SeriesCode)
--                  composite unique key on StandardsSeries -- matching the same looseness
--                  established by OSHARequirement.AppliesToModule -- so that requirements
--                  can be imported ahead of, or independently from, a fully-populated
--                  StandardsSeries catalog. ANSI/ASME/ISO are non-government consensus
--                  standards bodies whose standards are frequently incorporated by
--                  reference into OSHA/state regulations, but the standards text itself is
--                  copyrighted -- this table stores requirement metadata/technical summaries
--                  only, not verbatim copyrighted standard text. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StandardsRequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[StandardsRequirement] (
        [RequirementId]            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StandardsRequirement PRIMARY KEY
                                                       CONSTRAINT DF_StandardsRequirement_RequirementId DEFAULT (NEWID()),
        [FamilyCode]               NVARCHAR(10)     NOT NULL CONSTRAINT FK_StandardsRequirement_StandardsFamily REFERENCES [Compliance].[StandardsFamily] ([FamilyCode]),
        [SeriesCode]               NVARCHAR(30)     NOT NULL,
        [Title]                    NVARCHAR(300)    NOT NULL,
        [Description]              NVARCHAR(MAX)    NOT NULL,
        [ApplicabilityConditions]  NVARCHAR(MAX)    NULL,
        [ImpactedModules]          NVARCHAR(500)    NOT NULL,
        [Severity]                 NVARCHAR(20)     NOT NULL CONSTRAINT CK_StandardsRequirement_Severity
                                                       CHECK ([Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical')),
        [SourceUrl]                NVARCHAR(500)    NULL,
        [CreatedOn]                DATETIME2(3)     NOT NULL CONSTRAINT DF_StandardsRequirement_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                 ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_StandardsRequirement_FamilyCode] ON [Compliance].[StandardsRequirement] ([FamilyCode]);
    CREATE NONCLUSTERED INDEX [IX_StandardsRequirement_SeriesCode] ON [Compliance].[StandardsRequirement] ([SeriesCode]);
    CREATE NONCLUSTERED INDEX [IX_StandardsRequirement_Severity] ON [Compliance].[StandardsRequirement] ([Severity]);
END
GO

-- Additive-only evolution.

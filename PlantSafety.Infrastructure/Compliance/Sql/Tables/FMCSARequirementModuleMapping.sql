-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           FMCSARequirementModuleMapping.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many FMCSA-requirement-to-PlantSafety-module mapping, written by
--                  a deterministic mapping pass mirroring tools/OshaStandardsImport's Phase 6
--                  (Compliance.OSHARequirementModuleMapping). Exists because
--                  Compliance.FMCSARequirement.AppliesToModule is a single NVARCHAR column
--                  and a real requirement (e.g. a hours-of-service section) can legitimately
--                  apply to more than one module (Fleet/DriverQualification AND
--                  ComplianceCalendar). ModuleName is a real PlantSafety.Infrastructure
--                  top-level module folder name -- open vocabulary, no enum, matching OSHA's
--                  Module column. MappingRuleCode/Rationale make every mapping traceable back
--                  to the exact deterministic rule that produced it -- never a black box.
--                  Global reference data, no TenantId.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FMCSARequirementModuleMapping' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[FMCSARequirementModuleMapping] (
        [MappingId]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FMCSARequirementModuleMapping PRIMARY KEY CONSTRAINT DF_FMCSARequirementModuleMapping_MappingId DEFAULT (NEWID()),
        [RequirementId]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FMCSARequirementModuleMapping_FMCSARequirement REFERENCES [Compliance].[FMCSARequirement] ([RequirementId]),
        [ModuleName]       NVARCHAR(100)    NOT NULL,
        [MappingRuleCode]  NVARCHAR(100)    NOT NULL,
        [Rationale]        NVARCHAR(MAX)    NOT NULL,
        [Severity]         NVARCHAR(20)     NOT NULL,
        RowVersion         ROWVERSION       NOT NULL,
        CONSTRAINT UX_FMCSARequirementModuleMapping_Requirement_Module UNIQUE (RequirementId, ModuleName),
        CONSTRAINT CK_FMCSARequirementModuleMapping_Severity CHECK ([Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical'))
    );

    CREATE NONCLUSTERED INDEX [IX_FMCSARequirementModuleMapping_ModuleName] ON [Compliance].[FMCSARequirementModuleMapping] ([ModuleName]);
END
GO

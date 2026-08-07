-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           OSHARequirementModuleMapping.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Many-to-many OSHA-requirement-to-PlantSafety-module mapping, written by
--                  tools/OshaStandardsImport's deterministic mapping pass (Phase 6). Exists
--                  because Compliance.OSHARequirement.AppliesToModule is a single NVARCHAR
--                  column and a real requirement (e.g. a hot-work section) can legitimately
--                  apply to more than one module (PermitToWork AND MachineSafety). Exactly one
--                  row per requirement has IsPrimary = 1; that row's Module/Severity is mirrored
--                  onto OSHARequirement.AppliesToModule/Severity so the single-column read path
--                  built for the Fact Provider layer keeps working unchanged. RuleCode/Rationale
--                  make every mapping traceable back to the exact deterministic rule that
--                  produced it -- never a black box. Global reference data, no TenantId.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OSHARequirementModuleMapping' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[OSHARequirementModuleMapping] (
        [MappingId]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OSHARequirementModuleMapping PRIMARY KEY,
        [OSHARequirementId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_OSHARequirementModuleMapping_OSHARequirement REFERENCES [Compliance].[OSHARequirement] ([OSHARequirementId]),
        [Module]           NVARCHAR(100)    NOT NULL,
        [IsPrimary]        BIT              NOT NULL CONSTRAINT DF_OSHARequirementModuleMapping_IsPrimary DEFAULT (0),
        [Severity]         NVARCHAR(50)     NOT NULL,
        [MappingRuleCode]  NVARCHAR(100)    NOT NULL,
        [Rationale]        NVARCHAR(500)    NULL,
        [CreatedAtUtc]     DATETIME2(3)     NOT NULL CONSTRAINT DF_OSHARequirementModuleMapping_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UX_OSHARequirementModuleMapping_Requirement_Module UNIQUE (OSHARequirementId, Module)
    );

    CREATE NONCLUSTERED INDEX [IX_OSHARequirementModuleMapping_Module] ON [Compliance].[OSHARequirementModuleMapping] ([Module]);
    CREATE NONCLUSTERED INDEX [IX_OSHARequirementModuleMapping_OSHARequirementId] ON [Compliance].[OSHARequirementModuleMapping] ([OSHARequirementId]);
END
GO

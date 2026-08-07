-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           FMCSARequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Requirement rows derived directly from Compliance.FMCSAStandard section
--                  text (sentences containing "must"/"shall"/"required"/"prohibited" etc.).
--                  Not fabricated -- RequirementText always carries the raw source sentence.
--                  Global reference data, same scoping rules as FMCSAStandard (no TenantId).
--                  AppliesToModule is left NULL on import; PlantSafety modules map
--                  themselves to requirements later via Fact Providers / cross-domain
--                  rules, not at import time (mirrors Compliance.OSHARequirement).
--                  Severity is constrained to the shared ComplianceSeverity smart enum
--                  codes (PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FMCSARequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[FMCSARequirement] (
        [RequirementId]      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FMCSARequirement PRIMARY KEY CONSTRAINT DF_FMCSARequirement_RequirementId DEFAULT (NEWID()),
        [StandardId]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FMCSARequirement_FMCSAStandard REFERENCES [Compliance].[FMCSAStandard] ([StandardId]),
        [RequirementNumber]  NVARCHAR(50)     NULL,
        [RequirementText]    NVARCHAR(MAX)    NOT NULL,
        [Severity]           NVARCHAR(20)     NULL,
        [AppliesToModule]    NVARCHAR(100)    NULL,
        [Rationale]          NVARCHAR(MAX)    NULL,
        RowVersion           ROWVERSION       NOT NULL,
        CONSTRAINT CK_FMCSARequirement_Severity CHECK ([Severity] IS NULL OR [Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical'))
    );

    CREATE NONCLUSTERED INDEX [IX_FMCSARequirement_StandardId] ON [Compliance].[FMCSARequirement] ([StandardId]);
    CREATE NONCLUSTERED INDEX [IX_FMCSARequirement_AppliesToModule] ON [Compliance].[FMCSARequirement] ([AppliesToModule]);
END
GO

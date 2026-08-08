-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           EpaRegulatoryRequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Individual EPA regulatory requirement rows (citation + requirement text)
--                  keyed to an EpaProgram. Global regulatory reference data mirroring the
--                  OSHAStandard/OSHARequirement precedent, not tenant business data (no
--                  TenantId, no soft-delete). ImpactedModules is a free-text,
--                  comma-delimited list of PlantSafety module names (matching the
--                  OSHARequirement.AppliesToModule convention) rather than a DB enum, since
--                  a requirement can affect more than one module and the module set evolves
--                  independently of this reference table. Severity uses the exact codes of
--                  the shared PlantSafety.Domain.Compliance.RulesEngine.Enums.ComplianceSeverity
--                  smart enum (Advisory, Low, Medium, High, Critical). Uses the "Epa" prefix
--                  (not bare "EPA") to avoid collision with the pre-existing, unrelated
--                  Compliance.Certification.EPA / Compliance.RegulatoryCalendar.Deadlines.EPA
--                  features (CertificationEPA / DeadlinesEPA tables), which are not touched
--                  by this build. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EpaRegulatoryRequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[EpaRegulatoryRequirement] (
        [RequirementId]            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EpaRegulatoryRequirement PRIMARY KEY
                                                       CONSTRAINT DF_EpaRegulatoryRequirement_RequirementId DEFAULT (NEWID()),
        [ProgramCode]              NVARCHAR(20)     NOT NULL CONSTRAINT FK_EpaRegulatoryRequirement_EpaProgram REFERENCES [Compliance].[EpaProgram] ([ProgramCode]),
        [Title]                    NVARCHAR(300)    NOT NULL,
        [Citation]                 NVARCHAR(100)    NOT NULL,
        [Description]              NVARCHAR(MAX)    NOT NULL,
        [ApplicabilityConditions]  NVARCHAR(MAX)    NULL,
        [ImpactedModules]          NVARCHAR(500)    NOT NULL,
        [Severity]                 NVARCHAR(20)     NOT NULL CONSTRAINT CK_EpaRegulatoryRequirement_Severity
                                                       CHECK ([Severity] IN ('Advisory', 'Low', 'Medium', 'High', 'Critical')),
        [SourceUrl]                NVARCHAR(500)    NULL,
        [CreatedOn]                DATETIME2(3)     NOT NULL CONSTRAINT DF_EpaRegulatoryRequirement_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                 ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EpaRegulatoryRequirement_ProgramCode] ON [Compliance].[EpaRegulatoryRequirement] ([ProgramCode]);
    CREATE NONCLUSTERED INDEX [IX_EpaRegulatoryRequirement_Severity] ON [Compliance].[EpaRegulatoryRequirement] ([Severity]);
    CREATE NONCLUSTERED INDEX [IX_EpaRegulatoryRequirement_Citation] ON [Compliance].[EpaRegulatoryRequirement] ([Citation]);
END
GO

-- Additive-only evolution.

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Standards
--  Layer:          Infrastructure - SQL Scripts
--  File:           StandardsRiskAssessmentRule.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Risk-assessment methodology rows (ISO 12100, ANSI B11.0, ISO 45001,
--                  etc.) describing hazard-identification steps and risk-reduction
--                  measures, optionally tied back to a single StandardsRequirement. Global
--                  regulatory-adjacent reference data mirroring the OSHAStandard/
--                  OSHARequirement precedent, not tenant business data (no TenantId, no
--                  soft-delete). HazardIdentificationSteps and RiskReductionMeasures are
--                  list-valued fields with no child-table precedent for this class of
--                  table, so each is stored as a single NVARCHAR(MAX) column holding a
--                  native SQL Server JSON array of strings, validated with ISJSON,
--                  defaulting to an empty array -- matching the PHMSA_HazmatMaterial /
--                  EpaRecordkeepingRequirement JSON-array-column convention. ANSI/ASME/ISO
--                  are non-government consensus standards bodies whose standards are
--                  frequently incorporated by reference into OSHA/state regulations, but
--                  the standards text itself is copyrighted -- this table stores
--                  methodology metadata/technical summaries only, not verbatim copyrighted
--                  standard text. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StandardsRiskAssessmentRule' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[StandardsRiskAssessmentRule] (
        [RiskAssessmentRuleId]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_StandardsRiskAssessmentRule PRIMARY KEY
                                                           CONSTRAINT DF_StandardsRiskAssessmentRule_RiskAssessmentRuleId DEFAULT (NEWID()),
        [RequirementId]                UNIQUEIDENTIFIER NULL     CONSTRAINT FK_StandardsRiskAssessmentRule_StandardsRequirement REFERENCES [Compliance].[StandardsRequirement] ([RequirementId]),
        [Methodology]                  NVARCHAR(100)    NOT NULL,
        [HazardIdentificationSteps]    NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_StandardsRiskAssessmentRule_HazardIdentificationSteps DEFAULT (N'[]'),
        [RiskReductionMeasures]        NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_StandardsRiskAssessmentRule_RiskReductionMeasures DEFAULT (N'[]'),
        [Description]                  NVARCHAR(MAX)    NULL,
        [CreatedOn]                    DATETIME2(3)     NOT NULL CONSTRAINT DF_StandardsRiskAssessmentRule_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                     ROWVERSION       NOT NULL,
        CONSTRAINT CK_StandardsRiskAssessmentRule_HazardIdentificationSteps_Json CHECK (ISJSON([HazardIdentificationSteps]) = 1),
        CONSTRAINT CK_StandardsRiskAssessmentRule_RiskReductionMeasures_Json CHECK (ISJSON([RiskReductionMeasures]) = 1)
    );

    CREATE NONCLUSTERED INDEX [IX_StandardsRiskAssessmentRule_RequirementId] ON [Compliance].[StandardsRiskAssessmentRule] ([RequirementId]);
    CREATE NONCLUSTERED INDEX [IX_StandardsRiskAssessmentRule_Methodology] ON [Compliance].[StandardsRiskAssessmentRule] ([Methodology]);
END
GO

-- Additive-only evolution.

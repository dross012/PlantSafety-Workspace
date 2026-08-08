-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           EpaThresholdRule.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Numeric regulatory thresholds (quantities, time limits, reportable
--                  quantities) defined by an EPA program, optionally tied back to a single
--                  EpaRegulatoryRequirement (a threshold often but not always maps 1:1 to a
--                  requirement). Global regulatory reference data mirroring the
--                  OSHAStandard/OSHARequirement precedent, not tenant business data (no
--                  TenantId, no soft-delete). ConditionExpression is free text describing
--                  applicability (e.g. generator status) rather than a structured rule
--                  language, since threshold applicability conditions vary too widely
--                  across programs to model as a DB enum. Uses the "Epa" prefix (not bare
--                  "EPA") to avoid collision with the pre-existing, unrelated
--                  Compliance.Certification.EPA / Compliance.RegulatoryCalendar.Deadlines.EPA
--                  features (CertificationEPA / DeadlinesEPA tables), which are not touched
--                  by this build. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EpaThresholdRule' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[EpaThresholdRule] (
        [ThresholdRuleId]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EpaThresholdRule PRIMARY KEY
                                                       CONSTRAINT DF_EpaThresholdRule_ThresholdRuleId DEFAULT (NEWID()),
        [ProgramCode]              NVARCHAR(20)     NOT NULL CONSTRAINT FK_EpaThresholdRule_EpaProgram REFERENCES [Compliance].[EpaProgram] ([ProgramCode]),
        [RequirementId]            UNIQUEIDENTIFIER NULL     CONSTRAINT FK_EpaThresholdRule_EpaRegulatoryRequirement REFERENCES [Compliance].[EpaRegulatoryRequirement] ([RequirementId]),
        [ParameterName]            NVARCHAR(200)    NOT NULL,
        [LimitValue]               DECIMAL(18,4)    NOT NULL,
        [Unit]                     NVARCHAR(50)     NOT NULL,
        [ConditionExpression]      NVARCHAR(MAX)    NULL,
        [CreatedOn]                DATETIME2(3)     NOT NULL CONSTRAINT DF_EpaThresholdRule_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                 ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EpaThresholdRule_ProgramCode] ON [Compliance].[EpaThresholdRule] ([ProgramCode]);
    CREATE NONCLUSTERED INDEX [IX_EpaThresholdRule_RequirementId] ON [Compliance].[EpaThresholdRule] ([RequirementId]);
    CREATE NONCLUSTERED INDEX [IX_EpaThresholdRule_ParameterName] ON [Compliance].[EpaThresholdRule] ([ParameterName]);
END
GO

-- Additive-only evolution.

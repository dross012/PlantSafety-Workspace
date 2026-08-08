-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.EPA
--  Layer:          Infrastructure - SQL Scripts
--  File:           EpaReportingRequirement.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    EPA-mandated reporting triggers (release reporting, notifications,
--                  periodic filings) tied to an EPA program and optionally to a single
--                  EpaRegulatoryRequirement. Global regulatory reference data mirroring the
--                  OSHAStandard/OSHARequirement precedent, not tenant business data (no
--                  TenantId, no soft-delete). DeadlineDescription is intentionally free
--                  text (not a fixed numeric-days column) because EPA reporting deadlines
--                  are phrased inconsistently across programs (e.g. "immediately upon
--                  knowledge", "within 24 hours", "annually by March 1"), and forcing them
--                  into a single numeric interval would lose meaning or fabricate
--                  precision the underlying statute/regulation does not provide. Uses the
--                  "Epa" prefix (not bare "EPA") to avoid collision with the pre-existing,
--                  unrelated Compliance.Certification.EPA / Compliance.RegulatoryCalendar.
--                  Deadlines.EPA features (CertificationEPA / DeadlinesEPA tables), which
--                  are not touched by this build. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EpaReportingRequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[EpaReportingRequirement] (
        [ReportingRequirementId]  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EpaReportingRequirement PRIMARY KEY
                                                       CONSTRAINT DF_EpaReportingRequirement_ReportingRequirementId DEFAULT (NEWID()),
        [ProgramCode]              NVARCHAR(20)     NOT NULL CONSTRAINT FK_EpaReportingRequirement_EpaProgram REFERENCES [Compliance].[EpaProgram] ([ProgramCode]),
        [RequirementId]            UNIQUEIDENTIFIER NULL     CONSTRAINT FK_EpaReportingRequirement_EpaRegulatoryRequirement REFERENCES [Compliance].[EpaRegulatoryRequirement] ([RequirementId]),
        [TriggerDescription]       NVARCHAR(MAX)    NOT NULL,
        [DeadlineDescription]      NVARCHAR(300)    NOT NULL,
        [ReportingDestination]     NVARCHAR(300)    NOT NULL,
        [FormName]                 NVARCHAR(200)    NULL,
        [CreatedOn]                DATETIME2(3)     NOT NULL CONSTRAINT DF_EpaReportingRequirement_CreatedOn DEFAULT (SYSUTCDATETIME()),
        RowVersion                 ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EpaReportingRequirement_ProgramCode] ON [Compliance].[EpaReportingRequirement] ([ProgramCode]);
    CREATE NONCLUSTERED INDEX [IX_EpaReportingRequirement_RequirementId] ON [Compliance].[EpaReportingRequirement] ([RequirementId]);
END
GO

-- Additive-only evolution.

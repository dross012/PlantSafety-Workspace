-- ============================================================================
--  Project:        AI.SeverityAssessment -- Phase1-SeverityAssessmentLog
--  File:           01-table-severityassessmentlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AISeverityAssessment].[SeverityAssessmentLog]
--                  -- mirrors SeverityAssessmentOutput.cs exactly:
--                  BaseSeverity/DigitalTwinSimulatedSeverity/
--                  FinalAssessedSeverity (all FK'd to [Lookup].[SeverityLevel]),
--                  Notes (rule-triggered explanatory strings) serialized as
--                  JSON, plus the input scalars (HazardType/
--                  EmployeeInjuryCount/MachineFailureCount/EventChainLength
--                  -- SeverityAssessmentInput.cs) that produced them.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SeverityAssessmentLog' AND schema_id = SCHEMA_ID('AISeverityAssessment'))
BEGIN
    CREATE TABLE [AISeverityAssessment].[SeverityAssessmentLog]
    (
        [SeverityAssessmentLogId]      UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_SeverityAssessmentLog_Id DEFAULT NEWSEQUENTIALID(),
        [InputHazardTypeCode]           NVARCHAR(30)      NOT NULL,
        [InputEmployeeInjuryCount]       INT               NOT NULL,
        [InputMachineFailureCount]       INT               NOT NULL,
        [InputEventChainLength]          INT               NOT NULL,
        [BaseSeverityCode]               NVARCHAR(30)      NOT NULL,
        [DigitalTwinSimulatedSeverityCode] NVARCHAR(30)    NOT NULL,
        [FinalAssessedSeverityCode]      NVARCHAR(30)      NOT NULL,
        [NotesJson]                      NVARCHAR(MAX)     NOT NULL,
        [AssessedAtUtc]                  DATETIME2(3)      NOT NULL,

        [CreatedUtc]                     DATETIME2(3)      NOT NULL CONSTRAINT DF_SeverityAssessmentLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                      NVARCHAR(128)     NOT NULL CONSTRAINT DF_SeverityAssessmentLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                      BIT               NOT NULL CONSTRAINT DF_SeverityAssessmentLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_SeverityAssessmentLog PRIMARY KEY CLUSTERED ([SeverityAssessmentLogId]),
        CONSTRAINT FK_SeverityAssessmentLog_HazardType FOREIGN KEY ([InputHazardTypeCode]) REFERENCES [Lookup].[HazardType] ([Code]),
        CONSTRAINT FK_SeverityAssessmentLog_BaseSeverity FOREIGN KEY ([BaseSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT FK_SeverityAssessmentLog_TwinSeverity FOREIGN KEY ([DigitalTwinSimulatedSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT FK_SeverityAssessmentLog_FinalSeverity FOREIGN KEY ([FinalAssessedSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_SeverityAssessmentLog_NotesJson CHECK (ISJSON([NotesJson]) = 1)
    );
END
GO

-- ============================================================================
--  Project:        AI.Reasoning -- Phase1-ReasoningLog
--  File:           01-table-reasoningsessionlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIReasoning].[ReasoningSessionLog] --
--                  ReasoningSessionId (SafetyReasoningContext's own real
--                  session id) as the natural key, SeverityCode/
--                  DigitalTwinSimulatedSeverityCode as the two real input
--                  severity signals SafetyReasoningEngine.Reason worst-cases
--                  over, OverallSeverityCode as the real output
--                  (SafetyReasoningOutput.OverallSeverity), and
--                  ConclusionsJson as the output's real Conclusions list.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReasoningSessionLog' AND schema_id = SCHEMA_ID('AIReasoning'))
BEGIN
    CREATE TABLE [AIReasoning].[ReasoningSessionLog]
    (
        [ReasoningSessionLogId]              UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_ReasoningSessionLog_Id DEFAULT NEWSEQUENTIALID(),
        [ReasoningSessionId]                  UNIQUEIDENTIFIER  NOT NULL,
        [InputSeverityCode]                   NVARCHAR(30)      NOT NULL,
        [DigitalTwinSimulatedSeverityCode]     NVARCHAR(30)      NOT NULL,
        [OverallSeverityCode]                  NVARCHAR(30)      NOT NULL,
        [ConclusionsJson]                      NVARCHAR(MAX)     NOT NULL,
        [EvaluatedAtUtc]                       DATETIME2(3)      NOT NULL,

        [CreatedUtc]                           DATETIME2(3)      NOT NULL CONSTRAINT DF_ReasoningSessionLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                            NVARCHAR(128)     NOT NULL CONSTRAINT DF_ReasoningSessionLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                            BIT               NOT NULL CONSTRAINT DF_ReasoningSessionLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_ReasoningSessionLog PRIMARY KEY CLUSTERED ([ReasoningSessionLogId]),
        CONSTRAINT FK_ReasoningSessionLog_InputSeverity FOREIGN KEY ([InputSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT FK_ReasoningSessionLog_TwinSeverity FOREIGN KEY ([DigitalTwinSimulatedSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT FK_ReasoningSessionLog_OverallSeverity FOREIGN KEY ([OverallSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_ReasoningSessionLog_ConclusionsJson CHECK (ISJSON([ConclusionsJson]) = 1)
    );
END
GO

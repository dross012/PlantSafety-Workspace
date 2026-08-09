-- ============================================================================
--  Project:        AI.DigitalTwinLearning -- Phase1-DigitalTwinLearningLog
--  File:           01-table-digitaltwinlearninglog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIDigitalTwinLearning].
--                  [DigitalTwinLearningLog] -- one row per real
--                  DigitalTwinLearningEngine.Learn run. The five
--                  LearnedPattern results (HazardPropagation/
--                  SeverityEscalation/WorkflowBottleneck/EmployeeExposure/
--                  MachineFailure -- DigitalTwinLearningOutput.cs) are
--                  serialized as PatternsJson, and the rule-triggered model-
--                  update recommendations as RecommendationsJson, per this
--                  task's instruction.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DigitalTwinLearningLog' AND schema_id = SCHEMA_ID('AIDigitalTwinLearning'))
BEGIN
    CREATE TABLE [AIDigitalTwinLearning].[DigitalTwinLearningLog]
    (
        [DigitalTwinLearningLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_DigitalTwinLearningLog_Id DEFAULT NEWSEQUENTIALID(),
        [PatternsJson]               NVARCHAR(MAX)     NOT NULL,
        [RecommendationsJson]        NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]             DATETIME2(3)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)      NOT NULL CONSTRAINT DF_DigitalTwinLearningLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)     NOT NULL CONSTRAINT DF_DigitalTwinLearningLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                  BIT               NOT NULL CONSTRAINT DF_DigitalTwinLearningLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_DigitalTwinLearningLog PRIMARY KEY CLUSTERED ([DigitalTwinLearningLogId]),
        CONSTRAINT CK_DigitalTwinLearningLog_PatternsJson CHECK (ISJSON([PatternsJson]) = 1),
        CONSTRAINT CK_DigitalTwinLearningLog_RecommendationsJson CHECK (ISJSON([RecommendationsJson]) = 1)
    );
END
GO

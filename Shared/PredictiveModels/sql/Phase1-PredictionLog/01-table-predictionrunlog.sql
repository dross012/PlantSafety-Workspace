-- ============================================================================
--  Project:        Shared.PredictiveModels -- Phase1-PredictionLog
--  File:           01-table-predictionrunlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [PredictiveModels].[PredictionRunLog] --
--                  ONE shared append-only log table for all five Phase C.11
--                  model kinds (ModelKind discriminates: 'Hazard'/
--                  'Severity'/'Exposure'/'MachineFailure'/'EmployeeInjury'),
--                  rather than five near-identical tables, since every
--                  model's output is the same union of scalar fields
--                  (MovingAverage/TrendSlope/ZScoreOfLatest/PredictedValue/
--                  IsAnomalous/IsEscalating) per HazardLikelihoodPrediction/
--                  SeverityLikelihoodPrediction/ExposureLikelihoodPrediction/
--                  MachineFailurePrediction/EmployeeInjuryPrediction (all in
--                  Shared.PredictiveModels.Abstractions.Models). Input
--                  history (the rolling window of counts/weights fed to
--                  Predict) is logged as a JSON array column, per this
--                  task's "serialize nested lists as JSON" instruction.
--                  SubjectId (FacilityId/EmployeeId/MachineId, per ModelKind)
--                  is a nullable correlation column, not itself part of the
--                  C# input.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictionRunLog' AND schema_id = SCHEMA_ID('PredictiveModels'))
BEGIN
    CREATE TABLE [PredictiveModels].[PredictionRunLog]
    (
        [PredictionRunLogId]   UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_PredictionRunLog_Id DEFAULT NEWSEQUENTIALID(),
        [ModelKind]             NVARCHAR(30)      NOT NULL,
        [SubjectId]              UNIQUEIDENTIFIER  NULL,
        [InputHistoryJson]       NVARCHAR(MAX)     NOT NULL,
        [WindowSize]             INT               NOT NULL,
        [MovingAverage]          FLOAT             NOT NULL,
        [TrendSlope]             FLOAT             NULL,
        [ZScoreOfLatest]         FLOAT             NULL,
        [PredictedValue]         FLOAT             NULL,
        [IsAnomalous]            BIT               NULL,
        [IsEscalating]           BIT               NULL,
        [RunAtUtc]               DATETIME2(3)      NOT NULL,

        [CreatedUtc]             DATETIME2(3)      NOT NULL CONSTRAINT DF_PredictionRunLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]              NVARCHAR(128)     NOT NULL CONSTRAINT DF_PredictionRunLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]              BIT               NOT NULL CONSTRAINT DF_PredictionRunLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_PredictionRunLog PRIMARY KEY CLUSTERED ([PredictionRunLogId]),
        CONSTRAINT CK_PredictionRunLog_ModelKind CHECK ([ModelKind] IN
            (N'Hazard', N'Severity', N'Exposure', N'MachineFailure', N'EmployeeInjury')),
        CONSTRAINT CK_PredictionRunLog_InputHistoryJson CHECK (ISJSON([InputHistoryJson]) = 1)
    );
END
GO

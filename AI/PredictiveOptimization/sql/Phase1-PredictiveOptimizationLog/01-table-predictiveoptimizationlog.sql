-- ============================================================================
--  Project:        AI.PredictiveOptimization -- Phase1-PredictiveOptimizationLog
--  File:           01-table-predictiveoptimizationlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIPredictiveOptimization].
--                  [PredictiveOptimizationLog] -- one row per real
--                  PredictiveOptimizationEngine.Optimize run.
--                  PredictiveOptimizationOutput.cs's Models list (one
--                  PredictiveOptimizationModel per OptimizationTarget) is
--                  serialized as ModelsJson (per this task's instruction --
--                  a per-target normalized table would just duplicate
--                  Shared.PredictiveModels' Phase1-PredictionLog shape);
--                  RecommendedFocus/CompositeScore are the run's real
--                  scalar outputs, flattened as columns for direct
--                  querying.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveOptimizationLog' AND schema_id = SCHEMA_ID('AIPredictiveOptimization'))
BEGIN
    CREATE TABLE [AIPredictiveOptimization].[PredictiveOptimizationLog]
    (
        [PredictiveOptimizationLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_PredictiveOptimizationLog_Id DEFAULT NEWSEQUENTIALID(),
        [ModelsJson]                    NVARCHAR(MAX)     NOT NULL,
        [RecommendedFocus]               NVARCHAR(50)      NOT NULL,
        [CompositeScore]                 FLOAT             NOT NULL,
        [GeneratedAtUtc]                 DATETIME2(3)      NOT NULL,

        [CreatedUtc]                     DATETIME2(3)      NOT NULL CONSTRAINT DF_PredictiveOptimizationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                      NVARCHAR(128)     NOT NULL CONSTRAINT DF_PredictiveOptimizationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                      BIT               NOT NULL CONSTRAINT DF_PredictiveOptimizationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_PredictiveOptimizationLog PRIMARY KEY CLUSTERED ([PredictiveOptimizationLogId]),
        CONSTRAINT CK_PredictiveOptimizationLog_ModelsJson CHECK (ISJSON([ModelsJson]) = 1)
    );
END
GO

-- ============================================================================
--  Project:        AI.EnterpriseOrchestration -- Phase1-EnterpriseOrchestrationLog
--  File:           01-table-enterpriseorchestrationlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [AIEnterpriseOrchestration].
--                  [EnterpriseOrchestrationLog] -- one row per real
--                  EnterpriseOrchestrationEngine.Orchestrate run.
--                  FacilityCount/HighestSeverityCode are the run's real
--                  top-level scalars (EnterpriseOrchestrationOutput.cs);
--                  the three per-facility Guid lists
--                  (FacilitiesRequiringEvacuation/WithRegulatoryExposure/
--                  WithAnomalousPredictions) and the rule-triggered Alerts
--                  list are each serialized as their own JSON array column,
--                  per this task's instruction (each is a flat list of
--                  facility ids or strings, not a nested record needing
--                  normalization).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnterpriseOrchestrationLog' AND schema_id = SCHEMA_ID('AIEnterpriseOrchestration'))
BEGIN
    CREATE TABLE [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog]
    (
        [EnterpriseOrchestrationLogId]        UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_EnterpriseOrchestrationLog_Id DEFAULT NEWSEQUENTIALID(),
        [FacilityCount]                        INT               NOT NULL,
        [FacilitiesRequiringEvacuationJson]     NVARCHAR(MAX)     NOT NULL,
        [FacilitiesWithRegulatoryExposureJson]  NVARCHAR(MAX)     NOT NULL,
        [FacilitiesWithAnomalousPredictionsJson] NVARCHAR(MAX)    NOT NULL,
        [HighestSeverityCode]                   NVARCHAR(30)      NOT NULL,
        [AlertsJson]                             NVARCHAR(MAX)     NOT NULL,
        [GeneratedAtUtc]                         DATETIME2(3)      NOT NULL,

        [CreatedUtc]                             DATETIME2(3)      NOT NULL CONSTRAINT DF_EnterpriseOrchestrationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                              NVARCHAR(128)     NOT NULL CONSTRAINT DF_EnterpriseOrchestrationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                              BIT               NOT NULL CONSTRAINT DF_EnterpriseOrchestrationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_EnterpriseOrchestrationLog PRIMARY KEY CLUSTERED ([EnterpriseOrchestrationLogId]),
        CONSTRAINT FK_EnterpriseOrchestrationLog_HighestSeverity FOREIGN KEY ([HighestSeverityCode]) REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_EnterpriseOrchestrationLog_EvacuationJson CHECK (ISJSON([FacilitiesRequiringEvacuationJson]) = 1),
        CONSTRAINT CK_EnterpriseOrchestrationLog_RegulatoryJson CHECK (ISJSON([FacilitiesWithRegulatoryExposureJson]) = 1),
        CONSTRAINT CK_EnterpriseOrchestrationLog_AnomalousJson CHECK (ISJSON([FacilitiesWithAnomalousPredictionsJson]) = 1),
        CONSTRAINT CK_EnterpriseOrchestrationLog_AlertsJson CHECK (ISJSON([AlertsJson]) = 1)
    );
END
GO

-- ============================================================================
--  Project:        Shared.WorkflowEngine -- Phase1-WorkflowLog
--  File:           01-table-workflowevaluationlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [WorkflowEngine].[WorkflowEvaluationLog].
--                  One row per real WorkflowEngine.Evaluate(WorkflowContext)
--                  call: input scalars flattened as columns
--                  (HazardType/SeverityLevel/AffectedScope/
--                  EmployeeExposureCount/MachineFailureCount/
--                  EventChainLength -- WorkflowContext.cs), output as four
--                  JSON array columns (one per WorkflowDecision.cs list
--                  member). FacilityId is an optional caller-supplied
--                  correlation column (not itself part of WorkflowContext),
--                  since every real caller evaluates a workflow in the
--                  context of one real facility.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkflowEvaluationLog' AND schema_id = SCHEMA_ID('WorkflowEngine'))
BEGIN
    CREATE TABLE [WorkflowEngine].[WorkflowEvaluationLog]
    (
        [WorkflowEvaluationLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_WorkflowEvaluationLog_Id DEFAULT NEWSEQUENTIALID(),
        [FacilityId]                UNIQUEIDENTIFIER  NULL,
        [HazardTypeCode]            NVARCHAR(30)      NOT NULL,
        [SeverityCode]              NVARCHAR(30)      NOT NULL,
        [AffectedScope]             NVARCHAR(20)      NOT NULL,
        [EmployeeExposureCount]     INT               NOT NULL,
        [MachineFailureCount]       INT               NOT NULL,
        [EventChainLength]          INT               NOT NULL,
        [RequiredActionsJson]       NVARCHAR(MAX)     NOT NULL,
        [SopTriggersJson]           NVARCHAR(MAX)     NOT NULL,
        [PermitTriggersJson]        NVARCHAR(MAX)     NOT NULL,
        [EvacuationTriggersJson]    NVARCHAR(MAX)     NOT NULL,
        [NotificationRoutingsJson]  NVARCHAR(MAX)     NOT NULL,
        [EvaluatedAtUtc]            DATETIME2(3)      NOT NULL,

        [CreatedUtc]                DATETIME2(3)      NOT NULL CONSTRAINT DF_WorkflowEvaluationLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                 NVARCHAR(128)     NOT NULL CONSTRAINT DF_WorkflowEvaluationLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                 BIT               NOT NULL CONSTRAINT DF_WorkflowEvaluationLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_WorkflowEvaluationLog PRIMARY KEY CLUSTERED ([WorkflowEvaluationLogId]),
        CONSTRAINT FK_WorkflowEvaluationLog_HazardType FOREIGN KEY ([HazardTypeCode])
            REFERENCES [Lookup].[HazardType] ([Code]),
        CONSTRAINT FK_WorkflowEvaluationLog_SeverityLevel FOREIGN KEY ([SeverityCode])
            REFERENCES [Lookup].[SeverityLevel] ([Code]),
        CONSTRAINT CK_WorkflowEvaluationLog_AffectedScope CHECK ([AffectedScope] IN (N'Zone', N'Area', N'Building', N'Facility')),
        CONSTRAINT CK_WorkflowEvaluationLog_RequiredActionsJson CHECK (ISJSON([RequiredActionsJson]) = 1),
        CONSTRAINT CK_WorkflowEvaluationLog_SopTriggersJson CHECK (ISJSON([SopTriggersJson]) = 1),
        CONSTRAINT CK_WorkflowEvaluationLog_PermitTriggersJson CHECK (ISJSON([PermitTriggersJson]) = 1),
        CONSTRAINT CK_WorkflowEvaluationLog_EvacuationTriggersJson CHECK (ISJSON([EvacuationTriggersJson]) = 1),
        CONSTRAINT CK_WorkflowEvaluationLog_NotificationRoutingsJson CHECK (ISJSON([NotificationRoutingsJson]) = 1)
    );
END
GO

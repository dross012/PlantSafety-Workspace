-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling.PredictiveScheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           OptimizationRun.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the OptimizationRun aggregate root (IOptimizationRunRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Captures one invocation of
--                  the predictive scheduling optimization pipeline: the input task/availability
--                  snapshot (JSON), the requested objective, and -- once completed -- the winning
--                  strategy's result (JSON assignments/violations) so past optimization and
--                  reprioritization decisions become queryable/auditable rather than computed and
--                  discarded. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scheduling')
    EXEC('CREATE SCHEMA Scheduling AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OptimizationRun' AND schema_id = SCHEMA_ID('Scheduling'))
BEGIN
    CREATE TABLE [Scheduling].[OptimizationRun]
    (
        OptimizationRunId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OptimizationRun PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Objective               NVARCHAR(30)     NOT NULL,
        Status                  NVARCHAR(20)     NOT NULL,
        SelectedStrategyName    NVARCHAR(100)    NULL,
        Score                   FLOAT            NULL,
        FailureReason           NVARCHAR(1000)   NULL,
        InputTaskCount          INT              NOT NULL CONSTRAINT DF_OptimizationRun_InputTaskCount DEFAULT (0),
        InputAvailabilityCount  INT              NOT NULL CONSTRAINT DF_OptimizationRun_InputAvailabilityCount DEFAULT (0),
        AssignmentCount         INT              NOT NULL CONSTRAINT DF_OptimizationRun_AssignmentCount DEFAULT (0),
        UnscheduledTaskCount    INT              NOT NULL CONSTRAINT DF_OptimizationRun_UnscheduledTaskCount DEFAULT (0),
        ToleratedViolationCount INT              NOT NULL CONSTRAINT DF_OptimizationRun_ToleratedViolationCount DEFAULT (0),
        InputTasksJson          NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_OptimizationRun_InputTasksJson DEFAULT (N'[]'),
        InputAvailabilityJson   NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_OptimizationRun_InputAvailabilityJson DEFAULT (N'[]'),
        ResultAssignmentsJson   NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_OptimizationRun_ResultAssignmentsJson DEFAULT (N'[]'),
        UnscheduledTaskIdsJson  NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_OptimizationRun_UnscheduledTaskIdsJson DEFAULT (N'[]'),
        ToleratedViolationsJson NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_OptimizationRun_ToleratedViolationsJson DEFAULT (N'[]'),
        RequestedAtUtc          DATETIME2(3)     NOT NULL,
        CompletedAtUtc          DATETIME2(3)     NULL,
        RequestedBy             UNIQUEIDENTIFIER NULL,
        CorrelationId           UNIQUEIDENTIFIER NOT NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_OptimizationRun_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_OptimizationRun_TenantId_Site ON [Scheduling].[OptimizationRun] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_OptimizationRun_Status ON [Scheduling].[OptimizationRun] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_OptimizationRun_RequestedAtUtc ON [Scheduling].[OptimizationRun] (TenantId, RequestedAtUtc);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AITraining
--  Layer:          Infrastructure – SQL Scripts
--  File:           AITrainingJob.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the new Phase 4 AITrainingJob aggregate root, backing
--                  IAITrainingJobRepository (Domain write-side). Deliberately a distinct table
--                  from the pre-existing [Intelligence].[AITraining] read-model table (which backs
--                  the untouched date-range GetAITrainingQuery/usp_AITraining_GetTrainingJobs read
--                  path) -- this table carries the full lifecycle state
--                  (Queued/Running/Completed/Failed/Archived) plus a native RowVersion for
--                  optimistic-concurrency ETags. Mirrors [ContinuousImprovement].[RCA]'s
--                  Phase 4 additive-column precedent, but as a wholly new table per the task's
--                  "pick distinct proc/table names for your own new write paths" instruction.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AITrainingJob' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[AITrainingJob] (
        AITrainingJobId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        ModelName                NVARCHAR(200)    NOT NULL,
        ModelVersion              NVARCHAR(50)     NOT NULL,
        DatasetReference         NVARCHAR(400)    NULL,
        Status                   NVARCHAR(50)     NOT NULL,
        StartedAtUtc             DATETIME2(3)     NOT NULL,
        InitiatedBy              UNIQUEIDENTIFIER NOT NULL,
        RunStartedBy             UNIQUEIDENTIFIER NULL,
        RunStartedAtUtc          DATETIME2(3)     NULL,
        ModelArtifactReference   NVARCHAR(400)    NULL,
        CompletedAtUtc           DATETIME2(3)     NULL,
        FailureReason            NVARCHAR(MAX)    NULL,
        FailedAtUtc              DATETIME2(3)     NULL,
        ArchivedAtUtc            DATETIME2(3)     NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_AITrainingJob_IsDeleted DEFAULT (0),
        CreatedAtUtc             DATETIME2(3)     NOT NULL CONSTRAINT DF_AITrainingJob_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy                UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc            DATETIME2(3)     NULL,
        ModifiedBy               UNIQUEIDENTIFIER NULL,
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AITrainingJob_TenantId_SiteId] ON [Intelligence].[AITrainingJob] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AITrainingJob_StartedAtUtc] ON [Intelligence].[AITrainingJob] (StartedAtUtc);
END
GO

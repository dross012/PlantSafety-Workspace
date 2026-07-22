-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AITraining
--  Layer:          Infrastructure – SQL Scripts
--  File:           AITraining.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AITraining read-model, backing
--                  IAITrainingRepository.GetTrainingJobsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AITraining' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[AITraining] (
        TrainingJobId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        ModelName        NVARCHAR(200)    NOT NULL,
        ModelVersion     NVARCHAR(50)     NOT NULL,
        DatasetReference NVARCHAR(400)    NULL,
        Status           NVARCHAR(50)     NOT NULL,
        StartedAtUtc     DATETIME2(3)     NOT NULL,
        InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_AITraining_IsDeleted DEFAULT (0),
        CreatedAtUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_AITraining_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc    DATETIME2(3)     NULL,
        ModifiedBy       UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AITraining_TenantId_SiteId] ON [Intelligence].[AITraining] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AITraining_StartedAtUtc] ON [Intelligence].[AITraining] (StartedAtUtc);
END
GO

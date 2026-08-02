-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.DataLake
--  Layer:          Infrastructure – SQL Scripts
--  File:           DataLakeActivity.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the new Phase 4 DataLakeActivity aggregate root, backing
--                  IDataLakeActivityRepository (Domain write-side). Deliberately a distinct table
--                  from the pre-existing [Intelligence].[DataLake] read-model table (which backs
--                  the untouched date-range GetDataLakeQuery/usp_DataLake_GetDataLakeRecords read
--                  path) -- this table carries the full lifecycle state
--                  (Ingested/Processing/Curated/Failed/Archived) plus a native RowVersion for
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

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataLakeActivity' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[DataLakeActivity] (
        DataLakeActivityId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        DatasetName             NVARCHAR(200)    NOT NULL,
        Zone                    NVARCHAR(50)     NOT NULL,
        RecordCount              BIGINT           NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        ProcessedAtUtc          DATETIME2(3)     NOT NULL,
        ProcessedBy             UNIQUEIDENTIFIER NOT NULL,
        ProcessingStartedBy     UNIQUEIDENTIFIER NULL,
        ProcessingStartedAtUtc  DATETIME2(3)     NULL,
        CuratedAtUtc            DATETIME2(3)     NULL,
        FailureReason           NVARCHAR(MAX)    NULL,
        FailedAtUtc             DATETIME2(3)     NULL,
        ArchivedAtUtc           DATETIME2(3)     NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_DataLakeActivity_IsDeleted DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_DataLakeActivity_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy               UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc           DATETIME2(3)     NULL,
        ModifiedBy              UNIQUEIDENTIFIER NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DataLakeActivity_TenantId_SiteId] ON [Intelligence].[DataLakeActivity] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_DataLakeActivity_ProcessedAtUtc] ON [Intelligence].[DataLakeActivity] (ProcessedAtUtc);
END
GO

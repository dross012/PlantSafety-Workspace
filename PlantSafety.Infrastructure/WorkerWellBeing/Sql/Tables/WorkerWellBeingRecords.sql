-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure - SQL Scripts
--  File:           WorkerWellBeingRecords.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WorkerWellBeing aggregate backing
--                  IWorkerWellBeingRecordRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'WorkerWellBeing')
BEGIN
    EXEC('CREATE SCHEMA [WorkerWellBeing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkerWellBeingRecords' AND schema_id = SCHEMA_ID('WorkerWellBeing'))
BEGIN
    CREATE TABLE [WorkerWellBeing].[WorkerWellBeingRecords] (
        [WorkerWellBeingId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [WorkerId]          UNIQUEIDENTIFIER    NOT NULL,
        [WellBeingRiskLevel] NVARCHAR(20)       NOT NULL CONSTRAINT DF_WWB_RiskLevel DEFAULT ('Low'),
        [Status]            NVARCHAR(50)        NOT NULL CONSTRAINT DF_WWB_Status DEFAULT ('Open'),
        [RecordedAtUtc]     DATETIME2(3)        NOT NULL,
        [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
        [CreatedAt]         DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WorkerWellBeingRecords_TenantId_RecordedAtUtc]
        ON [WorkerWellBeing].[WorkerWellBeingRecords] ([TenantId], [RecordedAtUtc])
        INCLUDE ([WorkerId], [WellBeingRiskLevel], [Status]);
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkforceSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           WorkforceSafetyReadModel.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WorkforceSafetyReadModel read-model aggregate backing
--                  IWorkforceSafetyRepository.GetWorkforceSafetyAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkforceSafetyReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WorkforceSafetyReadModel] (
        [WorkforceSafetyRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [MetricId]                UNIQUEIDENTIFIER    NOT NULL,
        [MetricName]              NVARCHAR(400)       NOT NULL,
        [MetricValue]             DECIMAL(18,4)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [RecordedAtUtc]           DATETIME2(3)        NOT NULL,
        [RecordedBy]              UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WorkforceSafetyReadModel_TenantId_RecordedAtUtc]
        ON [Operational].[WorkforceSafetyReadModel] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[WorkforceSafetyReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[WorkforceSafetyReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
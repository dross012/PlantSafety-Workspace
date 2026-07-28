-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Training
--  Layer:          Infrastructure - SQL Scripts
--  File:           TrainingReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TrainingReadModel read-model aggregate backing
--                  ITrainingRepository.GetTrainingAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrainingReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[TrainingReadModel] (
        [TrainingRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [CourseName]              NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [CompletedAtUtc]  DATETIME2(3)        NOT NULL,
        [CertifiedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrainingReadModel_TenantId_CompletedAtUtc]
        ON [Operational].[TrainingReadModel] ([TenantId], [CompletedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[TrainingReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[TrainingReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

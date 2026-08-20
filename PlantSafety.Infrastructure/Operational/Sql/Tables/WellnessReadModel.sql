-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Wellness
--  Layer:          Infrastructure - SQL Scripts
--  File:           WellnessReadModel.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WellnessReadModel read-model aggregate backing
--                  IWellnessRepository.GetWellnessAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WellnessReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[WellnessReadModel] (
        [WellnessCheckInId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EmployeeId]              UNIQUEIDENTIFIER    NOT NULL,
        [EmployeeName]            NVARCHAR(400)       NOT NULL,
        [CheckInType]             NVARCHAR(400)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [CheckedInAtUtc]          DATETIME2(3)        NOT NULL,
        [CheckedInBy]             UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_WellnessReadModel_TenantId_CheckedInAtUtc]
        ON [Operational].[WellnessReadModel] ([TenantId], [CheckedInAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[WellnessReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[WellnessReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
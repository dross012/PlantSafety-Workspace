-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Classes
--  Layer:          Infrastructure - SQL Scripts
--  File:           ClassesReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ClassesReadModel read-model aggregate backing
--                  IClassesRepository.GetClassesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClassesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[ClassesReadModel] (
        [ClassId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [InstructorId]            UNIQUEIDENTIFIER    NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL

    );

    CREATE NONCLUSTERED INDEX [IX_ClassesReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[ClassesReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO

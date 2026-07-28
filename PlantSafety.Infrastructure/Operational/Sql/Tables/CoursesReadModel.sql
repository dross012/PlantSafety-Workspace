-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Courses
--  Layer:          Infrastructure - SQL Scripts
--  File:           CoursesReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CoursesReadModel read-model aggregate backing
--                  ICoursesRepository.GetCoursesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CoursesReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[CoursesReadModel] (
        [CourseId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(400)       NOT NULL,
        [Category]                NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [PublishedAtUtc]  DATETIME2(3)        NOT NULL,
        [PublishedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CoursesReadModel_TenantId_PublishedAtUtc]
        ON [Operational].[CoursesReadModel] ([TenantId], [PublishedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[CoursesReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[CoursesReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

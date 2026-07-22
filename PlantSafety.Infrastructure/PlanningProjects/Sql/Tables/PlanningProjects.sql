-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects.PlanningProjects
--  Layer:          Infrastructure – SQL Scripts
--  File:           PlanningProjects.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PlanningProjects read-model, backing
--                  IPlanningProjectsRepository.GetProjectsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PlanningProjects')
BEGIN
    EXEC('CREATE SCHEMA [PlanningProjects]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlanningProjects' AND schema_id = SCHEMA_ID('PlanningProjects'))
BEGIN
    CREATE TABLE [PlanningProjects].[PlanningProjects] (
        ProjectId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Name                  NVARCHAR(200)    NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        TargetCompletionUtc   DATETIME2(3)     NOT NULL,
        OwnerId               UNIQUEIDENTIFIER NOT NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_PlanningProjects_IsDeleted DEFAULT (0),
        CreatedAtUtc          DATETIME2(3)     NOT NULL CONSTRAINT DF_PlanningProjects_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy             UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc         DATETIME2(3)     NULL,
        ModifiedBy            UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PlanningProjects_TenantId_SiteId] ON [PlanningProjects].[PlanningProjects] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PlanningProjects_TargetCompletionUtc] ON [PlanningProjects].[PlanningProjects] (TargetCompletionUtc);
END
GO

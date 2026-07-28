-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.PlanningProjects.Projects
--  Layer:          Infrastructure – SQL Scripts
--  File:           Project.sql
--  Created:        2026-07-28
--  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the SafetyProject aggregate, backing IProjectRepository
--                  (usp_Project_Create/GetById). Additive-only evolution.
--
--                  Phase 3 SQL Modernization: this table/schema did not previously exist
--                  anywhere as real SQL -- ProjectDapperRepository referenced
--                  [PlanningProjects].[usp_Project_Create]/[usp_Project_GetById] against a
--                  schema/table that had never been created (the sibling
--                  PlanningProjects\Sql\Tables\Projects.sql / \Procedures\usp_Project_Insert.sql
--                  files are unrelated, unimplemented "TODO" template stubs never bound to any
--                  repository -- see this pass's dead-code findings). Includes
--                  RowVersion ROWVERSION NOT NULL from creation (no pre-existing concurrency
--                  column to preserve), exposed to callers as ETag via
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) in usp_Project_GetById,
--                  matching the RiskManagement/Financials-ROI precedent.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PlanningProjects')
BEGIN
    EXEC('CREATE SCHEMA [PlanningProjects]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Project' AND schema_id = SCHEMA_ID('PlanningProjects'))
BEGIN
    CREATE TABLE [PlanningProjects].[Project] (
        ProjectId            UNIQUEIDENTIFIER  NOT NULL PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER  NOT NULL,
        SiteId               UNIQUEIDENTIFIER  NOT NULL,
        Name                 NVARCHAR(200)     NOT NULL,
        Description          NVARCHAR(4000)    NOT NULL,
        Status               NVARCHAR(30)      NOT NULL CONSTRAINT DF_Project_Status DEFAULT (N'Draft'),
        Priority             NVARCHAR(20)      NOT NULL,
        ProjectOwnerId       UNIQUEIDENTIFIER  NOT NULL,
        ProjectManagerId     UNIQUEIDENTIFIER  NULL,
        PlannedStartUtc      DATETIMEOFFSET(3) NOT NULL,
        PlannedEndUtc        DATETIMEOFFSET(3) NOT NULL,
        ActualStartUtc       DATETIMEOFFSET(3) NULL,
        ActualEndUtc         DATETIMEOFFSET(3) NULL,
        PlannedBudget        DECIMAL(18,2)     NULL,
        OpenMilestoneCount   INT               NOT NULL CONSTRAINT DF_Project_OpenMilestoneCount DEFAULT (0),
        LastModifiedAtUtc    DATETIMEOFFSET(3) NOT NULL,
        IsDeleted            BIT               NOT NULL CONSTRAINT DF_Project_IsDeleted DEFAULT (0),
        CorrelationId        UNIQUEIDENTIFIER  NULL,
        CreatedAtUtc         DATETIME2(3)      NOT NULL CONSTRAINT DF_Project_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy            UNIQUEIDENTIFIER  NULL,
        ModifiedAtUtc        DATETIME2(3)      NULL,
        ModifiedBy           UNIQUEIDENTIFIER  NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Project_TenantId_SiteId] ON [PlanningProjects].[Project] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Project_TenantId_Status] ON [PlanningProjects].[Project] (TenantId, Status);
END
GO

IF COL_LENGTH('[PlanningProjects].[Project]', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [PlanningProjects].[Project] ADD RowVersion ROWVERSION NOT NULL;
END
GO

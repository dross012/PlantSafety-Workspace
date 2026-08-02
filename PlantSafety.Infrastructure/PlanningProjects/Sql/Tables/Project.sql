-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects
--  Layer:          Infrastructure – SQL Scripts
--  File:           Project.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Project aggregate root and its ProjectMilestone child
--                  collection, backing IProjectRepository (Domain write-side interface). Phase 4
--                  full build-out. Named [PlanningProjects].[PlanningProject] (NOT [Project]) to
--                  avoid colliding with the pre-existing, unrelated
--                  [PlanningProjects].[Project] table backing the LIVE
--                  Features.PlanningProjects.Handlers.Projects.IProjectRepository generic
--                  project-tracking vertical, and NOT [PlanningProjects] (plural) to avoid
--                  colliding with the pre-existing read-only [PlanningProjects].[PlanningProjects]
--                  table backing IPlanningProjectsRepository.GetProjectsAsync. TRY/CATCH +
--                  AuditLog/ErrorLog pattern lives in the stored procedures, not here.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PlanningProjects')
BEGIN
    EXEC('CREATE SCHEMA [PlanningProjects]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlanningProject' AND schema_id = SCHEMA_ID('PlanningProjects'))
BEGIN
    CREATE TABLE [PlanningProjects].[PlanningProject] (
        ProjectId               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Name                    NVARCHAR(200)    NOT NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        TargetCompletionUtc     DATETIME2(3)     NOT NULL,
        OwnerId                 UNIQUEIDENTIFIER NOT NULL,
        ActivatedBy             UNIQUEIDENTIFIER NULL,
        ActivatedAtUtc          DATETIME2(3)     NULL,
        PutOnHoldBy             UNIQUEIDENTIFIER NULL,
        PutOnHoldAtUtc          DATETIME2(3)     NULL,
        OnHoldReason            NVARCHAR(1000)   NULL,
        CompletedBy             UNIQUEIDENTIFIER NULL,
        CompletedAtUtc          DATETIME2(3)     NULL,
        CancelledBy             UNIQUEIDENTIFIER NULL,
        CancelledAtUtc          DATETIME2(3)     NULL,
        CancellationReason      NVARCHAR(1000)   NULL,
        ArchivedAtUtc           DATETIME2(3)     NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_PlanningProject_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_PlanningProject_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PlanningProject_TenantId_SiteId] ON [PlanningProjects].[PlanningProject] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PlanningProject_TargetCompletionUtc] ON [PlanningProjects].[PlanningProject] (TargetCompletionUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlanningProjectMilestone' AND schema_id = SCHEMA_ID('PlanningProjects'))
BEGIN
    CREATE TABLE [PlanningProjects].[PlanningProjectMilestone] (
        MilestoneId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ProjectId       UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        TargetDateUtc   DATETIME2(3)     NOT NULL,
        IsAchieved      BIT              NOT NULL CONSTRAINT DF_PlanningProjectMilestone_IsAchieved DEFAULT (0),
        CONSTRAINT FK_PlanningProjectMilestone_Project FOREIGN KEY (ProjectId)
            REFERENCES [PlanningProjects].[PlanningProject] (ProjectId)
    );

    CREATE NONCLUSTERED INDEX [IX_PlanningProjectMilestone_ProjectId] ON [PlanningProjects].[PlanningProjectMilestone] (ProjectId);
END
GO

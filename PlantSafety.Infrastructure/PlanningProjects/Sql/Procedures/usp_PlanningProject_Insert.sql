-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PlanningProject_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new Project aggregate root row (plus any milestones already present
--                  on the aggregate at creation time -- ordinarily none, since AddMilestone is a
--                  separate lifecycle action, but the proc accepts the milestone table type for
--                  completeness/idempotent re-sync). Backs IProjectRepository.AddAsync (Domain
--                  write-side repository). TRY/CATCH with AuditDB/ErrorDB logging per the
--                  canonical write-procedure template (mirrors usp_RootCauseAnalysis_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF TYPE_ID(N'[PlanningProjects].[PlanningProjectMilestoneTableType]') IS NULL
BEGIN
    EXEC('CREATE TYPE [PlanningProjects].[PlanningProjectMilestoneTableType] AS TABLE
    (
        MilestoneId     UNIQUEIDENTIFIER NOT NULL,
        Name            NVARCHAR(200)    NOT NULL,
        TargetDateUtc   DATETIME2(3)     NOT NULL,
        IsAchieved      BIT              NOT NULL
    )');
END
GO

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_PlanningProject_Insert]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Name NVARCHAR(200),
    @Status NVARCHAR(50),
    @TargetCompletionUtc DATETIME2(3),
    @OwnerId UNIQUEIDENTIFIER,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER,
    @Milestones [PlanningProjects].[PlanningProjectMilestoneTableType] READONLY
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PlanningProjects].[PlanningProject]
        (
            ProjectId, TenantId, SiteId, Name, Status, TargetCompletionUtc, OwnerId,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @ProjectId, @TenantId, @SiteId, @Name, @Status, @TargetCompletionUtc, @OwnerId,
            @CreatedAtUtc, @CreatedBy, 0
        );

        INSERT INTO [PlanningProjects].[PlanningProjectMilestone] (MilestoneId, ProjectId, Name, TargetDateUtc, IsAchieved)
        SELECT MilestoneId, @ProjectId, Name, TargetDateUtc, IsAchieved FROM @Milestones;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PlanningProject', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProjectId=', @ProjectId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProjectId=', @ProjectId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

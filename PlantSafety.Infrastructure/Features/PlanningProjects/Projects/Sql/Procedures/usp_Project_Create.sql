-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.PlanningProjects.Projects
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Project_Create.sql
--  Created:        2026-07-28
--  Author:         PlantSafety Platform Team (Claude Code migration-commander-agent)
--  SchemaVersion:  2026.1
--  Description:    Creates a SafetyProject in Draft status. Backs IProjectRepository.CreateAsync.
--                  TRY/CATCH with AuditLog/ErrorLog logging per the canonical write-procedure
--                  template (Phase 3 SQL Modernization -- this procedure/table did not previously
--                  exist anywhere as real SQL). Additive-only evolution. Schema-only: no
--                  @ETag input parameter or conflict-rejection logic is introduced here --
--                  IProjectRepository.CreateAsync has no ETag concept to enforce (matches
--                  RiskDapperRepository.AddAsync / RoiDapperRepository.CreateAsync precedent).
-- ============================================================================

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_Project_Create]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @ProjectId         UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER,
    @Name              NVARCHAR(200),
    @Description       NVARCHAR(4000),
    @Priority          NVARCHAR(20),
    @ProjectOwnerId    UNIQUEIDENTIFIER,
    @ProjectManagerId  UNIQUEIDENTIFIER = NULL,
    @PlannedStartUtc   DATETIME2(3),
    @PlannedEndUtc     DATETIME2(3),
    @PlannedBudget     DECIMAL(18,2) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [PlanningProjects].[Project]
            (ProjectId, TenantId, SiteId, Name, Description, Status, Priority, ProjectOwnerId, ProjectManagerId,
             PlannedStartUtc, PlannedEndUtc, ActualStartUtc, ActualEndUtc, PlannedBudget, OpenMilestoneCount,
             LastModifiedAtUtc, IsDeleted, CorrelationId, CreatedAtUtc, CreatedBy)
        VALUES
            (@ProjectId, @TenantId, @SiteId, @Name, @Description, N'Draft', @Priority, @ProjectOwnerId, @ProjectManagerId,
             @PlannedStartUtc, @PlannedEndUtc, NULL, NULL, @PlannedBudget, 0,
             @NowUtc, 0, @CorrelationId, @NowUtc, @ActorId);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Project', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProjectId=', @ProjectId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [PlanningProjects].[Project]
        WHERE ProjectId = @ProjectId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; ProjectId=', @ProjectId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO

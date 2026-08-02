-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PlanningProject_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Project aggregate root row by identity, tenant-scoped from
--                  day one, as two result sets (Project header, then its milestones). Backs
--                  IProjectRepository.GetByIdAsync (Domain write-side aggregate hydration) and the
--                  Application-layer GetProjectByIdQuery read path. Canonical TRY/CATCH + ErrorDB
--                  logging (no AuditLog for reads). Projects RowVersion as a string ETag. Mirrors
--                  usp_RootCauseAnalysis_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_PlanningProject_GetById]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ProjectId, TenantId, SiteId, Name, Status, TargetCompletionUtc, OwnerId,
            ActivatedBy, ActivatedAtUtc, PutOnHoldBy, PutOnHoldAtUtc, OnHoldReason,
            CompletedBy, CompletedAtUtc, CancelledBy, CancelledAtUtc, CancellationReason,
            ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [PlanningProjects].[PlanningProject]
        WHERE ProjectId = @ProjectId AND TenantId = @TenantId AND IsDeleted = 0;

        SELECT MilestoneId, ProjectId, Name, TargetDateUtc, IsAchieved
        FROM [PlanningProjects].[PlanningProjectMilestone]
        WHERE ProjectId = @ProjectId;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProjectId=', @ProjectId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

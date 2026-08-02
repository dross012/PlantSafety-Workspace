-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PreventiveMaintenance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PreventiveMaintenanceTask_GetById.sql
--  Created:        2026-07-30
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single PreventiveMaintenanceTask aggregate root row by identity,
--                  tenant-scoped from day one. Backs IPreventiveMaintenanceTaskRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer
--                  GetPreventiveMaintenanceTaskByIdQuery read path (via
--                  PreventiveMaintenanceTaskReadDapperRepository). Canonical TRY/CATCH + ErrorDB
--                  logging (no AuditLog for reads). Projects RowVersion as a string ETag
--                  (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so callers can capture
--                  the current concurrency token for read-before-write flows. Distinct from the
--                  pre-existing usp_PreventiveMaintenance_List (date-range list query, left
--                  untouched). Mirrors usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_PreventiveMaintenanceTask_GetById]
    @PreventiveMaintenanceTaskId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            PreventiveMaintenanceTaskId, TenantId, SiteId, Status, EquipmentId, TaskDescription, ScheduledAtUtc,
            StartedBy, StartedAtUtc, CompletedBy, CompletionNotes, CompletedAtUtc,
            VerifiedBy, VerifiedAtUtc, CancelledBy, CancelledReason, CancelledAtUtc, OverdueAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Operational].[PreventiveMaintenanceReadModel]
        WHERE PreventiveMaintenanceTaskId = @PreventiveMaintenanceTaskId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PreventiveMaintenanceTaskId=', @PreventiveMaintenanceTaskId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

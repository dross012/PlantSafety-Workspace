-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkerWellBeingAggregate_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single WorkerWellBeing aggregate root row by identity,
--                  tenant-scoped from day one. Backs IWorkerWellBeingRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetWorkerWellBeingByIdQuery
--                  read path. Canonical TRY/CATCH + ErrorDB logging (no AuditLog for reads).
--                  Projects RowVersion as a string ETag. Mirrors usp_AuditingAggregate_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [WorkerWellBeing].[usp_WorkerWellBeingAggregate_GetById]
    @WorkerWellBeingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            WorkerWellBeingId, TenantId, SiteId, WorkerId, WellBeingRiskLevel, CheckInType,
            WellBeingScore, FollowUpRequired, Status, RecordedAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [WorkerWellBeing].[WorkerWellBeingAggregate]
        WHERE WorkerWellBeingId = @WorkerWellBeingId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

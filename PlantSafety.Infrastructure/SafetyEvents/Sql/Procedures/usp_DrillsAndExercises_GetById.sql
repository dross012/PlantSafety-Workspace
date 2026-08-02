-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_DrillsAndExercises_GetById.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single DrillsAndExercises aggregate root row by identity,
--                  tenant-scoped from day one. Backs IDrillsAndExercisesRepository.GetByIdAsync
--                  (both the Domain write-side aggregate-hydration repository and the
--                  Application-layer GetDrillsAndExercisesByIdQuery read path). Canonical
--                  TRY/CATCH + ErrorDB logging (no AuditLog for reads). Projects RowVersion as a
--                  string ETag (CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)) so
--                  callers can capture the current concurrency token for read-before-write flows.
--                  Aliases the physical DrillId primary-key column to DrillsAndExercisesId so the
--                  pre-existing list proc's column name is left completely undisturbed. Mirrors
--                  usp_Permits_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_DrillsAndExercises_GetById]
    @DrillsAndExercisesId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            DrillId AS DrillsAndExercisesId, TenantId, SiteId, Status, DrillType, ParticipantCount, ScheduledAtUtc,
            ConductedAtUtc, ConductedBy, CompletedBy, CompletedAtUtc, DebriefNotes,
            CancelledBy, CancelledReason, CancelledAtUtc, ClosedBy, ClosedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [SafetyEvents].[DrillsAndExercises]
        WHERE DrillId = @DrillsAndExercisesId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DrillsAndExercisesId=', @DrillsAndExercisesId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO

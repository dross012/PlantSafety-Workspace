-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_DrillsAndExercises_Insert.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Inserts a new DrillsAndExercises aggregate root row. Backs
--                  IDrillsAndExercisesRepository.AddAsync. TRY/CATCH with AuditDB/ErrorDB logging
--                  per the canonical write-procedure template (mirrors usp_Permits_Insert.sql).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_DrillsAndExercises_Insert]
    @DrillsAndExercisesId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DrillType NVARCHAR(50),
    @ParticipantCount INT,
    @ScheduledAtUtc DATETIME2(3) = NULL,
    @ConductedAtUtc DATETIME2(3) = NULL,
    @ConductedBy UNIQUEIDENTIFIER = NULL,
    @CompletedBy UNIQUEIDENTIFIER = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @DebriefNotes NVARCHAR(2000) = NULL,
    @CancelledBy UNIQUEIDENTIFIER = NULL,
    @CancelledReason NVARCHAR(2000) = NULL,
    @CancelledAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[DrillsAndExercises]
        (
            DrillId, TenantId, SiteId, Status, DrillType, ParticipantCount, ScheduledAtUtc,
            ConductedAtUtc, ConductedBy, CompletedBy, CompletedAtUtc, DebriefNotes,
            CancelledBy, CancelledReason, CancelledAtUtc, ClosedBy, ClosedAtUtc, ArchivedAtUtc,
            CreatedAtUtc, CreatedBy, IsDeleted
        )
        VALUES
        (
            @DrillsAndExercisesId, @TenantId, @SiteId, @Status, @DrillType, @ParticipantCount, @ScheduledAtUtc,
            @ConductedAtUtc, @ConductedBy, @CompletedBy, @CompletedAtUtc, @DebriefNotes,
            @CancelledBy, @CancelledReason, @CancelledAtUtc, @ClosedBy, @ClosedAtUtc, @ArchivedAtUtc,
            @CreatedAtUtc, @CreatedBy, 0
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'DrillsAndExercises', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'DrillsAndExercisesId=', @DrillsAndExercisesId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DrillsAndExercisesId=', @DrillsAndExercisesId, N'; User=', SYSTEM_USER);
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

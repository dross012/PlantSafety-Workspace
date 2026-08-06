-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Operational.Training
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Training_Assign.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [Operational].[TrainingAssignment] row. Backs
--                  ITrainingRepository.AssignAsync / TrainingDapperRepository.AssignAsync.
--                  Parameter set matches StoredProcedureBuilder.WithCanonicalHeaders plus the
--                  explicit Add* calls in TrainingDapperRepository.AssignAsync exactly. Status
--                  starts fixed at 'Assigned'. TRY/CATCH with AuditDB/ErrorDB logging per the
--                  established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Training_Assign]
    @TenantId            UNIQUEIDENTIFIER,
    @CorrelationId       UNIQUEIDENTIFIER,
    @ActorId             UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2(3),
    @RecordId             UNIQUEIDENTIFIER,
    @WorkerId             UNIQUEIDENTIFIER,
    @TrainingCourseCode   NVARCHAR(50),
    @TrainingCourseName   NVARCHAR(200),
    @DeliveryMethod       NVARCHAR(50),
    @DueDateUtc           DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Operational].[TrainingAssignment]
        (
            RecordId, TenantId, WorkerId, TrainingCourseCode, TrainingCourseName, DeliveryMethod,
            Status, DueDateUtc,
            CreatedAtUtc, CreatedBy, LastModifiedByActorId, LastModifiedAtUtc
        )
        VALUES
        (
            @RecordId, @TenantId, @WorkerId, @TrainingCourseCode, @TrainingCourseName, @DeliveryMethod,
            N'Assigned', @DueDateUtc,
            @NowUtc, @ActorId, @ActorId, @NowUtc
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'TrainingAssignment', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RecordId=', @RecordId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RecordId=', @RecordId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

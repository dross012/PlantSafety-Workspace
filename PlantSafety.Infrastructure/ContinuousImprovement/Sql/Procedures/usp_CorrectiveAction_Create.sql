-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CorrectiveAction_Create.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [ContinuousImprovement].[CorrectiveAction] row. Backs
--                  ICorrectiveActionRepository.CreateAsync / CorrectiveActionDapperRepository.
--                  CreateAsync. Parameter set matches StoredProcedureBuilder.WithCanonicalHeaders
--                  (@TenantId/@CorrelationId/@ActorId/@NowUtc) plus the explicit Add* calls in
--                  CorrectiveActionDapperRepository.CreateAsync exactly. @NowUtc (app-supplied) is
--                  used for all business timestamp columns instead of SYSUTCDATETIME() to keep the
--                  write deterministic/replayable; SYSUTCDATETIME() is retained only for the
--                  best-effort AuditLog/ErrorLog metadata timestamps, matching the
--                  usp_Incident_Create / usp_Accident_Insert / usp_CorrectiveAction_Upsert
--                  precedent for that distinction. New rows start in Status = 'Open' (matches
--                  CorrectiveActionDto.Status doc comment). TRY/CATCH with AuditDB/ErrorDB logging
--                  per the established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_Create]
    @TenantId           UNIQUEIDENTIFIER,
    @CorrelationId      UNIQUEIDENTIFIER,
    @ActorId            UNIQUEIDENTIFIER,
    @NowUtc             DATETIME2(3),
    @CaId               UNIQUEIDENTIFIER,
    @SourceEventId      UNIQUEIDENTIFIER,
    @SourceEventType    NVARCHAR(100),
    @Title              NVARCHAR(200),
    @Description        NVARCHAR(4000),
    @ActionType         NVARCHAR(50),
    @Priority           NVARCHAR(50),
    @AssignedToWorkerId UNIQUEIDENTIFIER,
    @DueAtUtc           DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [ContinuousImprovement].[CorrectiveAction]
        (
            CaId, TenantId, SourceEventId, SourceEventType, Title, Description, ActionType,
            Priority, AssignedToWorkerId, DueAtUtc, Status, CompletedAtUtc,
            IsDeleted, DeletedAtUtc, DeletionReason,
            LastModifiedByActorId, LastModifiedAtUtc, CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @CaId, @TenantId, @SourceEventId, @SourceEventType, @Title, @Description, @ActionType,
            @Priority, @AssignedToWorkerId, @DueAtUtc, N'Open', NULL,
            0, NULL, NULL,
            @ActorId, @NowUtc, @NowUtc, @ActorId
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CorrectiveAction', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

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

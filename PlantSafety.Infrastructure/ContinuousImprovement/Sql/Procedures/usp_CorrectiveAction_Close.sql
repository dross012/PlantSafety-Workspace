-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CorrectiveAction_Close.sql
--  Created:        2026-08-06
--  SchemaVersion:  2026.1
--  Description:    Transitions [ContinuousImprovement].[CorrectiveAction].Status to Closed
--                  (terminal state). Backs ICorrectiveActionRepository.UpdateAsync /
--                  CorrectiveActionDapperRepository.UpdateAsync (Close transition). No ETag
--                  concurrency check, matching the usp_Incident_Resolve/usp_Incident_Escalate
--                  lifecycle-transition precedent (the domain layer's own status-guard in
--                  CorrectiveAction.Close already rejects invalid source states before this proc is
--                  ever called). TRY/CATCH with AuditDB/ErrorDB logging per the established pattern.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_Close]
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @CaId          UNIQUEIDENTIFIER,
    @Status        NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [ContinuousImprovement].[CorrectiveAction]
        SET
            Status = @Status,
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc = @NowUtc
        WHERE CaId = @CaId
          AND TenantId = @TenantId
          AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CorrectiveAction', DB_NAME(), N'CLOSE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CaId=', @CaId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
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

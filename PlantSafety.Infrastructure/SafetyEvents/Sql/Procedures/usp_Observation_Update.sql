-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Observation_Update.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Partial update of [SafetyEvents].[Observation] mutable fields. Backs
--                  IObservationRepository.UpdateAsync / ObservationDapperRepository.UpdateAsync.
--                  Per IObservationRepository's own doc comment ("only non-null values are
--                  applied"), each mutable column uses COALESCE(@Param, Column). @ETag is
--                  mandatory and matched against the native ROWVERSION; zero rows affected
--                  (returned via @@ROWCOUNT, required because the caller must invoke this via
--                  ExecuteScalarAsync -- see ObservationDapperRepository.UpdateAsync) signals a
--                  stale ETag or a missing/foreign-tenant row. TRY/CATCH with AuditDB/ErrorDB
--                  logging per the established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Observation_Update]
    @TenantId        UNIQUEIDENTIFIER,
    @CorrelationId   UNIQUEIDENTIFIER,
    @ActorId         UNIQUEIDENTIFIER,
    @NowUtc          DATETIME2(3),
    @ObservationId   UNIQUEIDENTIFIER,
    @Title           NVARCHAR(200) = NULL,
    @Description     NVARCHAR(4000) = NULL,
    @ObservationType NVARCHAR(30) = NULL,
    @ETag            NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SafetyEvents].[Observation]
        SET
            Title             = COALESCE(@Title, Title),
            Description       = COALESCE(@Description, Description),
            ObservationType   = COALESCE(@ObservationType, ObservationType),
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc = @NowUtc
        WHERE ObservationId = @ObservationId
          AND TenantId = @TenantId
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Observation', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ObservationId=', @ObservationId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
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

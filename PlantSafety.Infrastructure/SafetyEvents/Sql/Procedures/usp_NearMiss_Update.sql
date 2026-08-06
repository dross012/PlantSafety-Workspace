-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMiss_Update.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Partial update of [SafetyEvents].[NearMiss] mutable fields. Backs
--                  INearMissRepository.UpdateAsync / NearMissDapperRepository.UpdateAsync. Per
--                  INearMissRepository's own doc comment ("only non-null values are applied"),
--                  each mutable column uses COALESCE(@Param, Column) so a NULL parameter leaves the
--                  existing value untouched (mirrors usp_ComplianceResources_BulkUpdate's COALESCE
--                  pattern). @ETag is mandatory (NearMissDapperRepository always supplies it) and is
--                  matched against the native ROWVERSION; zero rows affected (returned via
--                  @@ROWCOUNT) signals either a stale ETag or a missing/foreign-tenant row --
--                  NearMissDapperRepository.UpdateAsync throws ConcurrencyConflictException on 0,
--                  so both cases are deliberately folded into the same signal rather than
--                  distinguished server-side. TRY/CATCH with AuditDB/ErrorDB logging per the
--                  established pattern. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMiss_Update]
    @TenantId          UNIQUEIDENTIFIER,
    @CorrelationId     UNIQUEIDENTIFIER,
    @ActorId           UNIQUEIDENTIFIER,
    @NowUtc            DATETIME2(3),
    @NearMissId        UNIQUEIDENTIFIER,
    @Title             NVARCHAR(200)  = NULL,
    @Description       NVARCHAR(4000) = NULL,
    @PotentialSeverity NVARCHAR(50)   = NULL,
    @HazardCategory    NVARCHAR(100)  = NULL,
    @ETag              NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SafetyEvents].[NearMiss]
        SET
            Title              = COALESCE(@Title, Title),
            Description        = COALESCE(@Description, Description),
            PotentialSeverity  = COALESCE(@PotentialSeverity, PotentialSeverity),
            HazardCategory     = COALESCE(@HazardCategory, HazardCategory),
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc  = @NowUtc
        WHERE NearMissId = @NearMissId
          AND TenantId = @TenantId
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'NearMiss', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'NearMissId=', @NearMissId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'NearMissId=', @NearMissId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
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

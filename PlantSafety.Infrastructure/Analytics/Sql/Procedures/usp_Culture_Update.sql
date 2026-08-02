-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Culture_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a Culture aggregate root row. Backs
--                  ICultureRepository.UpdateAsync. @TenantId is part of the WHERE clause and
--                  @ETag enforces optimistic concurrency against the native RowVersion column,
--                  mirroring usp_Permits_Update.sql. @RowsAffected is captured via @@ROWCOUNT
--                  immediately after the UPDATE (before the audit INSERT, which would otherwise
--                  reset it) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the @ETag
--                  concurrency check. Phase 4 full build-out: Culture previously had no update
--                  path at all (ICultureRepository was Create/Read-only).
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_Update]
    @CultureId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @Status             NVARCHAR(30),
    @SurveyPeriod       NVARCHAR(20),
    @CultureIndexScore  DECIMAL(5,2),
    @EngagementScore    DECIMAL(5,2),
    @ParticipationRate  DECIMAL(5,2),
    @TopRiskTheme       NVARCHAR(200),
    @ReviewedByUserId   UNIQUEIDENTIFIER = NULL,
    @ReviewedAtUtc      DATETIME2(3)     = NULL,
    @PublishedByUserId  UNIQUEIDENTIFIER = NULL,
    @PublishedAtUtc     DATETIME2(3)     = NULL,
    @ClosedByUserId     UNIQUEIDENTIFIER = NULL,
    @ClosedAtUtc        DATETIME2(3)     = NULL,
    @ArchivedAtUtc      DATETIME2(3)     = NULL,
    @UpdatedBy          UNIQUEIDENTIFIER = NULL,
    @ETag               NVARCHAR(64)     = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Culture] WHERE CultureId = @CultureId AND TenantId = @TenantId)
            THROW 50004, N'Culture record not found.', 1;

        UPDATE [Analytics].[Culture]
        SET
            Status = @Status,
            SurveyPeriod = @SurveyPeriod,
            CultureIndexScore = @CultureIndexScore,
            EngagementScore = @EngagementScore,
            ParticipationRate = @ParticipationRate,
            TopRiskTheme = @TopRiskTheme,
            ReviewedByUserId = @ReviewedByUserId,
            ReviewedAtUtc = @ReviewedAtUtc,
            PublishedByUserId = @PublishedByUserId,
            PublishedAtUtc = @PublishedAtUtc,
            ClosedByUserId = @ClosedByUserId,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE CultureId = @CultureId AND TenantId = @TenantId
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Culture', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CultureId=', @CultureId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; User=', SYSTEM_USER);
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

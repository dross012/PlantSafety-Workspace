-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Culture_Insert.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing ICultureRepository.AddAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-27 -- rewritten from a plain TRANSACTION/ROLLBACK shortcut to the full
--                  canonical pattern used platform-wide (see usp_Incident_Create.sql): TRY/CATCH,
--                  a best-effort AuditDB.AuditLog insert on success, and structured ErrorDB.ErrorLog
--                  logging + THROW on failure. No @CorrelationId/@ActorId parameters exist here --
--                  unlike IIncidentRepository.CreateAsync, ICultureRepository.AddAsync's signature
--                  is deliberately thin (Task AddAsync(Culture, CancellationToken) only, per
--                  docs/architecture/step-a-iculturerepository-implementation-plan.md) and the
--                  Culture aggregate does not expose CorrelationId as a public property, so the
--                  audit/error context strings below reference @CultureId only. @CreatedBy is
--                  used as the audit actor (equivalent to @ActorId elsewhere), since for a
--                  Create-only operation the creator and the acting user are the same identity.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_Insert]
    @CultureId          UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @SiteId             UNIQUEIDENTIFIER = NULL,
    @Status             NVARCHAR(30),
    @SurveyPeriod       NVARCHAR(20),
    @CultureIndexScore  DECIMAL(5,2),
    @EngagementScore    DECIMAL(5,2),
    @ParticipationRate  DECIMAL(5,2),
    @TopRiskTheme       NVARCHAR(200),
    @CreatedAtUtc       DATETIME2(3),
    @CreatedBy          UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Analytics].[Culture]
        (
            CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
            ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @CultureId, @TenantId, @SiteId, @Status, @SurveyPeriod, @CultureIndexScore, @EngagementScore,
            @ParticipationRate, @TopRiskTheme, @CreatedAtUtc, @CreatedBy
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Culture', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CultureId=', @CultureId, N'; TenantId=', @TenantId, N'; CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
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

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CultureAssessment_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [SafetyCulture].[CultureAssessment],
--                  backing ICultureAssessmentRepository.UpdateAsync (RecordScore/
--                  PublishReport lifecycle transitions). Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_CultureAssessment_Update]
    @TenantId                  UNIQUEIDENTIFIER,
    @CorrelationId             UNIQUEIDENTIFIER,
    @ActorId                   UNIQUEIDENTIFIER,
    @NowUtc                    DATETIME2(3),
    @CultureAssessmentId       UNIQUEIDENTIFIER,
    @Status                    NVARCHAR(20),
    @OverallCultureScore       DECIMAL(5,2) = NULL,
    @LeadershipEngagementScore DECIMAL(5,2) = NULL,
    @Summary                   NVARCHAR(2000),
    @PublishedAtUtc            DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [SafetyCulture].[CultureAssessment]
        SET [Status]                    = @Status,
            [OverallCultureScore]        = @OverallCultureScore,
            [LeadershipEngagementScore]  = @LeadershipEngagementScore,
            [Summary]                    = @Summary,
            [PublishedAtUtc]             = @PublishedAtUtc,
            [LastModifiedAtUtc]          = @NowUtc
        WHERE [CultureAssessmentId] = @CultureAssessmentId
          AND [TenantId]            = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'CultureAssessment', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'CultureAssessmentId=', @CultureAssessmentId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureAssessmentId=', @CultureAssessmentId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO

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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

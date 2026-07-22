-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CultureAssessment_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [SafetyCulture].[CultureAssessment], backing
--                  ICultureAssessmentRepository.AddAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_CultureAssessment_Create]
    @TenantId                      UNIQUEIDENTIFIER,
    @CorrelationId                 UNIQUEIDENTIFIER,
    @ActorId                       UNIQUEIDENTIFIER,
    @NowUtc                        DATETIME2(3),
    @CultureAssessmentId           UNIQUEIDENTIFIER,
    @SiteId                        UNIQUEIDENTIFIER = NULL,
    @Status                        NVARCHAR(20),
    @AssessmentTitle               NVARCHAR(200),
    @PeriodStartUtc                DATETIME2(3),
    @PeriodEndUtc                  DATETIME2(3),
    @OverallCultureScore           DECIMAL(5,2) = NULL,
    @LeadershipEngagementScore     DECIMAL(5,2) = NULL,
    @Summary                       NVARCHAR(2000),
    @RelatedRiskManagementHazardId UNIQUEIDENTIFIER = NULL,
    @CreatedAtUtc                  DATETIME2(3),
    @CreatedBy                     UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [SafetyCulture].[CultureAssessment]
            ([CultureAssessmentId], [TenantId], [SiteId], [Status], [AssessmentTitle],
             [PeriodStartUtc], [PeriodEndUtc], [OverallCultureScore], [LeadershipEngagementScore],
             [Summary], [RelatedRiskManagementHazardId], [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@CultureAssessmentId, @TenantId, @SiteId, @Status, @AssessmentTitle,
             @PeriodStartUtc, @PeriodEndUtc, @OverallCultureScore, @LeadershipEngagementScore,
             @Summary, @RelatedRiskManagementHazardId, @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

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
BEGIN TRY
    BEGIN TRANSACTION;

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

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

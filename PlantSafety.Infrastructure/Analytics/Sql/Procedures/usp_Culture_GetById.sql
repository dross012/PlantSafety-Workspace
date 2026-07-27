-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Culture_GetById.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ICultureRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetById]
    @TenantId  UNIQUEIDENTIFIER,
    @CultureId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
        ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy
    FROM [Analytics].[Culture]
    WHERE CultureId = @CultureId AND TenantId = @TenantId;
END
GO

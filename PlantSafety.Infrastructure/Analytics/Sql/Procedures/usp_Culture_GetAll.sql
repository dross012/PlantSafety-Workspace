-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Culture_GetAll.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ICultureRepository.GetAllAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-27 -- added ETag, same pattern/rationale as usp_Culture_GetById.sql.
--  UPDATED:        2026-08-02 -- Phase 4 full build-out: projects the new Review/Publish/Close/
--                  Archive lifecycle columns, mirroring usp_Culture_GetById.sql.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetAll]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
        ParticipationRate, TopRiskTheme, ReviewedByUserId, ReviewedAtUtc, PublishedByUserId,
        PublishedAtUtc, ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy,
        UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[Culture]
    WHERE TenantId = @TenantId
    ORDER BY CreatedAtUtc DESC;
END
GO

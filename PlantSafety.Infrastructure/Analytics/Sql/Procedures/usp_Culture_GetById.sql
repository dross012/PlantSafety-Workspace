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
--  UPDATED:        2026-07-27 -- added ETag, projected from the new RowVersion column the same
--                  way as usp_IncidentRead_GetById.sql: CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1) -- the intermediate CAST is required, SQL Server rejects a
--                  direct CONVERT from rowversion/timestamp to a character type even with an
--                  explicit style code (see docs/architecture/step-b-rowversion-conversion-fix-plan.md).
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Culture_GetById]
    @TenantId  UNIQUEIDENTIFIER,
    @CultureId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CultureId, TenantId, SiteId, Status, SurveyPeriod, CultureIndexScore, EngagementScore,
        ParticipationRate, TopRiskTheme, CreatedAtUtc, CreatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[Culture]
    WHERE CultureId = @CultureId AND TenantId = @TenantId;
END
GO

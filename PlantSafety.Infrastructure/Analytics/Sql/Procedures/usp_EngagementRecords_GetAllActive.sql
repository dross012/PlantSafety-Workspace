-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Engagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EngagementRecords_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Engagement rows. Backs IEngagementRepository.ListAsync,
--                  whose EngagementSpecification predicate is applied in-memory over the candidate
--                  set returned here (the Domain interface carries no tenant/site scoping
--                  parameter to push down to SQL). Mirrors usp_Permits_GetAllActive.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_EngagementRecords_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        EngagementId, TenantId, SiteId, Status, WorkerId, EngagementScore, SurveyPeriod, ParticipationChannel,
        ResponseCount, LastEngagedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[EngagementRecords]
    WHERE IsDeleted = 0;
END
GO

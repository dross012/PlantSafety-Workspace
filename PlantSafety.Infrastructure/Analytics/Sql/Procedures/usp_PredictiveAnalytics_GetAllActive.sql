-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.PredictiveAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PredictiveAnalytics_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PredictiveAnalytics rows. Backs
--                  IPredictiveAnalyticsRepository.ListAsync, whose PredictiveAnalyticsSpecification
--                  predicate is applied in-memory over the candidate set returned here (the
--                  Domain interface carries no tenant/site scoping parameter to push down to SQL).
--                  Mirrors usp_Permits_GetAllActive.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_PredictiveAnalytics_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        PredictionId, TenantId, SiteId, Status, ModelName, PredictionType, TargetEntityId, Score, ConfidenceLevel,
        PredictionHorizonDays, GeneratedAtUtc, GeneratedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[PredictiveAnalytics]
    WHERE IsDeleted = 0;
END
GO

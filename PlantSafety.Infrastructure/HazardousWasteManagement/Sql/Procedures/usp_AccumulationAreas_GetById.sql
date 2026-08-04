-- ============================================================================
--  Module: HazardousWasteManagement.AccumulationAreas | Layer: Infrastructure - SQL Scripts
--  File: usp_AccumulationAreas_GetById.sql | Created: 2026-08-04
--  Description: Returns a single AccumulationAreas row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_GetById]
(
    @AccumulationAreaId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[AccumulationAreas]
    WHERE [AccumulationAreaId] = @AccumulationAreaId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

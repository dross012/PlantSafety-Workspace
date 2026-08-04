-- ============================================================================
--  Module: HazardousWasteManagement.AccumulationAreas | Layer: Infrastructure - SQL Scripts
--  File: usp_AccumulationAreas_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted AccumulationAreas rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[AccumulationAreas]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

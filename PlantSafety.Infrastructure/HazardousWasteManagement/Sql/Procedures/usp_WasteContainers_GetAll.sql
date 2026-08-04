-- ============================================================================
--  Module: HazardousWasteManagement.WasteContainers | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteContainers_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted WasteContainers rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[WasteContainers]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

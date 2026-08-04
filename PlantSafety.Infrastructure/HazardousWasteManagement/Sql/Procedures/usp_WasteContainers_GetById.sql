-- ============================================================================
--  Module: HazardousWasteManagement.WasteContainers | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteContainers_GetById.sql | Created: 2026-08-04
--  Description: Returns a single WasteContainers row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_GetById]
(
    @WasteContainerId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[WasteContainers]
    WHERE [WasteContainerId] = @WasteContainerId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

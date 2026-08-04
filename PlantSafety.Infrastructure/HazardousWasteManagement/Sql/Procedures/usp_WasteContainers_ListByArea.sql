-- ============================================================================
--  Module: HazardousWasteManagement.WasteContainers | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteContainers_ListByArea.sql | Created: 2026-08-04
--  Description: Read-only procedure backing IWasteContainerRepository.
--               ListContainersByAccumulationAreaAsync (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_ListByArea]
    @TenantId UNIQUEIDENTIFIER,
    @AccumulationAreaId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [WasteContainerId], [TenantId], [SiteId], [AccumulationAreaId], [WasteStreamId],
        [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [ClosedAtUtc], [Status]
    FROM [HazardousWasteManagement].[WasteContainers]
    WHERE TenantId = @TenantId
      AND AccumulationAreaId = @AccumulationAreaId
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY OpenedAtUtc DESC;
END
GO

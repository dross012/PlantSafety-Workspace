-- ============================================================================
--  Module: HazardousWasteManagement.AccumulationAreas | Layer: Infrastructure - SQL Scripts
--  File: usp_AccumulationAreas_ListNearingTimeLimit.sql | Created: 2026-08-04
--  Description: Read-only procedure backing IAccumulationAreaRepository.
--               ListAccumulationAreasNearingTimeLimitAsync. Joins open WasteContainers to find
--               areas with a container within 10 days of its regulatory 90/180-day deadline.
--               Feeds the Accumulation Area Time-Limit Board dashboard. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_ListNearingTimeLimit]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        a.[AccumulationAreaId], a.[TenantId], a.[SiteId], a.[AreaName], a.[LocationDescription], a.[TimeLimitDays], a.[Status],
        MIN(DATEADD(DAY, c.[RegulatoryTimeLimitDays], c.[OpenedAtUtc])) AS [EarliestContainerDeadlineUtc]
    FROM [HazardousWasteManagement].[AccumulationAreas] a
    INNER JOIN [HazardousWasteManagement].[WasteContainers] c
        ON c.[AccumulationAreaId] = a.[AccumulationAreaId]
       AND c.[Status] = 'Open'
       AND ISNULL(c.[IsDeleted], 0) = 0
    WHERE a.TenantId = @TenantId
      AND (@SiteId IS NULL OR a.SiteId = @SiteId)
      AND ISNULL(a.[IsDeleted], 0) = 0
      AND DATEADD(DAY, c.[RegulatoryTimeLimitDays], c.[OpenedAtUtc]) <= DATEADD(DAY, 10, SYSUTCDATETIME())
    GROUP BY a.[AccumulationAreaId], a.[TenantId], a.[SiteId], a.[AreaName], a.[LocationDescription], a.[TimeLimitDays], a.[Status]
    ORDER BY [EarliestContainerDeadlineUtc] ASC;
END
GO

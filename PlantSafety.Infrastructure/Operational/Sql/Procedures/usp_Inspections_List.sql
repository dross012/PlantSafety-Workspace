-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Inspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Inspections_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[InspectionsReadModel],
--                  backing IInspectionsRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Inspections_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InspectionId],
        [TenantId],
        [SiteId],
        [InspectionType],
        [Outcome],
        [Status],
        [InspectedAtUtc],
        [InspectedBy]
FROM [Operational].[InspectionsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [InspectedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [InspectedAtUtc] DESC;
GO

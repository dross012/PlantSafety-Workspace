-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.HazardousMaterials
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardousMaterials_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[HazardousMaterialsReadModel],
--                  backing IHazardousMaterialsRepository.GetHazardousMaterialsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_HazardousMaterials_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [HazardousMaterialId],
        [TenantId],
        [SiteId],
        [MaterialName],
        [HazardClass],
        [Status],
        [RegisteredAtUtc],
        [RegisteredBy]
FROM [Operational].[HazardousMaterialsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RegisteredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RegisteredAtUtc] DESC;
GO

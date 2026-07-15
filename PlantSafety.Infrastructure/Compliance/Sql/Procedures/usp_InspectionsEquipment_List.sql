-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsEquipment
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_InspectionsEquipment_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[InspectionsEquipment],
--                  backing IEquipmentRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_InspectionsEquipment_List]
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
        [EquipmentCategory],
        [ScheduledAtUtc],
        [Status],
        [Inspector]
FROM [Compliance].[InspectionsEquipment]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
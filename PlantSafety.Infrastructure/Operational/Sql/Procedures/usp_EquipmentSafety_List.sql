-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.EquipmentSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EquipmentSafety_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[EquipmentSafetyReadModel],
--                  backing IEquipmentSafetyRepository.GetEquipmentSafetyAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_EquipmentSafety_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [EquipmentSafetyRecordId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [EquipmentName],
        [Status],
        [InspectedAtUtc],
        [InspectedBy]
FROM [Operational].[EquipmentSafetyReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [InspectedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [InspectedAtUtc] DESC;
GO

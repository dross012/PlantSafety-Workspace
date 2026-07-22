-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TwoHandControls_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ITwoHandControlsRepository.GetTwoHandControlsItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_TwoHandControls_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [TwoHandControlsId],
        [TenantId],
        [SiteId],
        [ControlTitle],
        [EquipmentType] AS [ControlType],
        [EquipmentLocation],
        [Status] AS [ControlStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [MachineSafety].[TwoHandControls]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

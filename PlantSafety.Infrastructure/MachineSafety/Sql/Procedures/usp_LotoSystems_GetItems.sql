-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.LotoSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoSystems_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ILotoSystemsRepository.GetLotoSystemsItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_LotoSystems_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [LotoSystemsId],
        [TenantId],
        [SiteId],
        [SystemTitle],
        [EnergySourceTypes] AS [EnergySourceType],
        [IsolationPointDescription],
        [Status] AS [SystemStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [MachineSafety].[LotoSystems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

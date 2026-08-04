-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICraneHoistSystemsRepository.GetCraneHoistSystemsItemsAsync (Application
--                  read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CraneHoistSystemsId],
        [TenantId],
        [SiteId],
        [AssetTag],
        [AssetTitle],
        [AssetType],
        [RatedLoadCapacityKg],
        [LastObservedLoadKg],
        [OverloadEventCount],
        [InspectionDueAtUtc],
        [OperatorQualificationExpiresAtUtc],
        [Status] AS [SystemStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [MachineSafety].[CraneHoistSystems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

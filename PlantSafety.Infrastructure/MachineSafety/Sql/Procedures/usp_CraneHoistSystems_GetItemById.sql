-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_GetItemById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICraneHoistSystemsRepository.GetCraneHoistSystemsItemByIdAsync (Application
--                  read-model, single-record lookup used by GetCraneHoistAssetByIdQuery,
--                  ListLiftEventsByAssetQuery, and GetInspectionHistoryQuery).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetItemById]
    @TenantId UNIQUEIDENTIFIER,
    @CraneHoistSystemsId UNIQUEIDENTIFIER
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
      AND CraneHoistSystemsId = @CraneHoistSystemsId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO

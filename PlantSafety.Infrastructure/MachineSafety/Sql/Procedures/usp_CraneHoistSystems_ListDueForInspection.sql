-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_ListDueForInspection.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICraneHoistSystemsRepository.ListAssetsDueForInspectionAsync. Returns assets
--                  whose scheduled inspection due date has arrived, or whose status already
--                  reflects an inspection-due/overdue condition. Supports the module's recurring
--                  inspection-scheduling hook. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_ListDueForInspection]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @AsOfUtc  DATETIME2(3)
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
      AND (
            [Status] IN (N'InspectionDue', N'InspectionOverdue')
            OR ([InspectionDueAtUtc] IS NOT NULL AND [InspectionDueAtUtc] <= @AsOfUtc)
          )
    ORDER BY [InspectionDueAtUtc] ASC;
END
GO

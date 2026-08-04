-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_ListExpiringQualifications.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICraneHoistSystemsRepository.ListExpiringOperatorQualificationsAsync. Returns
--                  assets whose current, non-suspended operator qualification expires within the
--                  given day window. Supports the module's qualification-renewal reminder hook.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_ListExpiringQualifications]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @WithinDays INT
AS
SET NOCOUNT ON;
BEGIN
    DECLARE @NowUtc DATETIME2(3) = SYSUTCDATETIME();
    DECLARE @HorizonUtc DATETIME2(3) = DATEADD(DAY, @WithinDays, @NowUtc);

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
      AND ISNULL([OperatorQualificationSuspended], 0) = 0
      AND [OperatorQualificationExpiresAtUtc] IS NOT NULL
      AND [OperatorQualificationExpiresAtUtc] BETWEEN @NowUtc AND @HorizonUtc
    ORDER BY [OperatorQualificationExpiresAtUtc] ASC;
END
GO

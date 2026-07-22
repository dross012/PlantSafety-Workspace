-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EmergencyPreparedness
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyPreparedness_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IEmergencyPreparednessRepository.GetEmergencyPreparednessItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EmergencyPreparedness_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [EmergencyPreparednessId],
        [TenantId],
        [SiteId],
        [PlanTitle],
        [EmergencyType] AS [EmergencyType],
        [EvacuationRoute],
        [Status] AS [PreparednessStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [EmployeeSafety].[EmergencyPreparedness]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

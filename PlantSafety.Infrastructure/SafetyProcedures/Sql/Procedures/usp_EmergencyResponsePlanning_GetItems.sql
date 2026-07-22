-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.EmergencyResponsePlanning
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EmergencyResponsePlanning_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IEmergencyResponsePlanningRepository.GetEmergencyResponsePlanningItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_EmergencyResponsePlanning_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [EmergencyResponsePlanningId],
        [TenantId],
        [SiteId],
        [PlanTitle],
        [EmergencyType] AS [EmergencyType],
        [EvacuationRoute],
        [Status] AS [PlanStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [SafetyProcedures].[EmergencyResponsePlanning]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

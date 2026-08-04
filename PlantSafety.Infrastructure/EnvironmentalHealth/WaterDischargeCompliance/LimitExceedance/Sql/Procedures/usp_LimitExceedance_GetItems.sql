-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LimitExceedance_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only procedure backing ILimitExceedanceRepository.ListActiveExceedancesAsync (status not Closed). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT [LimitExceedanceId], [TenantId], [SiteId], [DischargePointId], [ParameterName], [MeasuredValue],
           [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc], [Status], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[LimitExceedance]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] <> N'Closed'
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [DetectedAtUtc] DESC;
END
GO
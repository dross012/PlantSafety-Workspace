-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EffluentSample_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only procedure backing IEffluentSampleRepository.GetSampleHistoryByPointAsync (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_GetItems]
    @TenantId         UNIQUEIDENTIFIER,
    @DischargePointId UNIQUEIDENTIFIER,
    @FromUtc          DATETIME2(3),
    @ToUtc            DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT [EffluentSampleId], [TenantId], [SiteId], [DischargePointId], [Parameter], [MeasuredValue],
           [UnitOfMeasure], [SampledAtUtc], [LabSourceSystem], [ChainOfCustodyReference], [CorrectionOfSampleId],
           [Status], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[EffluentSample]
    WHERE [TenantId] = @TenantId
      AND [DischargePointId] = @DischargePointId
      AND [SampledAtUtc] >= @FromUtc
      AND [SampledAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [SampledAtUtc] DESC;
END
GO
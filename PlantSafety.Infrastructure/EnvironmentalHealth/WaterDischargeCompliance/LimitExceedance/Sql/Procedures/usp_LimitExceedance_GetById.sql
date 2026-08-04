-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LimitExceedance_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single LimitExceedance row by identifier, backing ILimitExceedanceRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_GetById]
(
    @LimitExceedanceId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LimitExceedanceId], [TenantId], [SiteId], [Status], [DischargePointId], [EffluentSampleId], [DischargeLimitId],
           [ParameterName], [MeasuredValue], [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc],
           [RepeatExceedanceCount], [ResponseInitiatedAtUtc], [ResponseInitiatedByUserId], [ResponseNotes],
           [ClosedAtUtc], [ClosedByUserId], [ResolutionNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[LimitExceedance]
    WHERE [LimitExceedanceId] = @LimitExceedanceId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
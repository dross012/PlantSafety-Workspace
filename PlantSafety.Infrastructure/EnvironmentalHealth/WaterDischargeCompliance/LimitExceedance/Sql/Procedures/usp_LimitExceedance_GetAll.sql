-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LimitExceedance_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted LimitExceedance rows, backing ILimitExceedanceRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LimitExceedanceId], [TenantId], [SiteId], [Status], [DischargePointId], [EffluentSampleId], [DischargeLimitId],
           [ParameterName], [MeasuredValue], [LimitValue], [ExceedancePercentage], [Severity], [DetectedAtUtc],
           [RepeatExceedanceCount], [ResponseInitiatedAtUtc], [ResponseInitiatedByUserId], [ResponseNotes],
           [ClosedAtUtc], [ClosedByUserId], [ResolutionNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[LimitExceedance]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
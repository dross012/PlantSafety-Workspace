-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EffluentSample_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EffluentSample rows, backing IEffluentSampleRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [EffluentSampleId], [TenantId], [SiteId], [Status], [DischargePointId], [Parameter], [MeasuredValue],
           [UnitOfMeasure], [SampledAtUtc], [CollectedByUserId], [LabSourceSystem], [ChainOfCustodyReference],
           [CorrectionOfSampleId], [CorrectionReason], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[EffluentSample]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
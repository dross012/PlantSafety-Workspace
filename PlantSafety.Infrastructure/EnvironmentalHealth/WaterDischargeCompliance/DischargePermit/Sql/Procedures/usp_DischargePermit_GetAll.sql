-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePermit_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted DischargePermit rows, backing IDischargePermitRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargePermitId], [TenantId], [SiteId], [Status], [PermitNumber], [RegulatoryProgram], [IssuingAuthority],
           [EffectiveAtUtc], [ExpiresAtUtc], [RenewalCount], [PermitConditionSummary], [LifecycleActionReason],
           [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargePermit]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
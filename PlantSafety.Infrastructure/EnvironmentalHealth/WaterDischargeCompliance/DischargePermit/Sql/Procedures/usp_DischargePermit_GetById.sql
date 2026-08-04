-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePermit_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single DischargePermit row by identifier, backing IDischargePermitRepository.GetByIdAsync and the read-model GetDischargePermitByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePermit_GetById]
(
    @DischargePermitId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargePermitId], [TenantId], [SiteId], [Status], [PermitNumber], [RegulatoryProgram], [IssuingAuthority],
           [EffectiveAtUtc], [ExpiresAtUtc], [RenewalCount], [PermitConditionSummary], [LifecycleActionReason],
           [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargePermit]
    WHERE [DischargePermitId] = @DischargePermitId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
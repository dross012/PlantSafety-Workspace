-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsWorkstationDesign_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single ErgonomicsWorkstationDesign row by identifier, backing
--                  IErgonomicsWorkstationDesignRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_ErgonomicsWorkstationDesign_GetById]
(
    @ErgonomicsWorkstationDesignId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ErgonomicsWorkstationDesignId], [TenantId], [SiteId], [Status], [ProcedureTitle], [DesignCategory], [WorkstationLocation], [ApprovedByUserId], [AssessedAtUtc], [NextAssessmentDueAtUtc], [RiskFactorCount], [ErgonomicsScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[ErgonomicsWorkstationDesign]
    WHERE [ErgonomicsWorkstationDesignId] = @ErgonomicsWorkstationDesignId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

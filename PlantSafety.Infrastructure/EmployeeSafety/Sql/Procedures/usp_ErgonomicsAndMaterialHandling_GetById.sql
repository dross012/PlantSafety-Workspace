-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsAndMaterialHandling_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single ErgonomicsAndMaterialHandling row by identifier, backing
--                  IErgonomicsAndMaterialHandlingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_ErgonomicsAndMaterialHandling_GetById]
(
    @ErgonomicsAndMaterialHandlingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ErgonomicsAndMaterialHandlingId], [TenantId], [SiteId], [Status], [AssessmentTitle], [HazardCategory], [WorkstationLocation], [AssessedByUserId], [AssessedAtUtc], [RiskFactorCount], [RiskScore], [ControlsDescription], [FollowUpDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[ErgonomicsAndMaterialHandling]
    WHERE [ErgonomicsAndMaterialHandlingId] = @ErgonomicsAndMaterialHandlingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

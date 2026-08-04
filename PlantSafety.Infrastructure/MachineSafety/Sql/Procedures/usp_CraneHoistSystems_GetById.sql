-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single CraneHoistSystems row by identifier, backing
--                  ICraneHoistSystemsRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetById]
(
    @CraneHoistSystemsId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[CraneHoistSystems]
    WHERE [CraneHoistSystemsId] = @CraneHoistSystemsId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

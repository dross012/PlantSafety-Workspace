-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.CraneHoistSystems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_CraneHoistSystems_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted CraneHoistSystems rows, backing
--                  ICraneHoistSystemsRepository.ListAsync (the Domain specification predicate
--                  carries no server-side-translatable filter, so it is applied in-memory in
--                  the Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_CraneHoistSystems_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CraneHoistSystemsId], [TenantId], [SiteId], [Status], [AssetTag], [AssetTitle], [AssetType], [RatedLoadCapacityKg], [LastObservedLoadKg], [LastLiftAtUtc], [OverloadEventCount], [OverloadFlagged], [InspectionDueAtUtc], [LastInspectionAtUtc], [LastInspectionPassed], [OperatorUserId], [OperatorQualifiedAtUtc], [OperatorQualificationExpiresAtUtc], [OperatorQualificationSuspended], [LastRiggingInspectionAtUtc], [LastRiggingDefectDetected], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[CraneHoistSystems]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

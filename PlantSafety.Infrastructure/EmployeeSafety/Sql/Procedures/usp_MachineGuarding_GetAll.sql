-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.MachineGuarding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuarding_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted MachineGuarding rows, backing
--                  IMachineGuardingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_MachineGuarding_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardingId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentTag], [Location], [HasInterlock], [InspectionIntervalDays], [LastInspectedByUserId], [LastInspectedAtUtc], [NextInspectionDueAtUtc], [OpenDeficiencyCount], [GuardConditionScore], [BypassDetected], [LastFindingNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[MachineGuarding]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

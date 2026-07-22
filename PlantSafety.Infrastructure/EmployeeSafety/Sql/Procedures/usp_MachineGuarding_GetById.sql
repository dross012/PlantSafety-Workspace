-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.MachineGuarding
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuarding_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single MachineGuarding row by identifier, backing
--                  IMachineGuardingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_MachineGuarding_GetById]
(
    @MachineGuardingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardingId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentTag], [Location], [HasInterlock], [InspectionIntervalDays], [LastInspectedByUserId], [LastInspectedAtUtc], [NextInspectionDueAtUtc], [OpenDeficiencyCount], [GuardConditionScore], [BypassDetected], [LastFindingNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[MachineGuarding]
    WHERE [MachineGuardingId] = @MachineGuardingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

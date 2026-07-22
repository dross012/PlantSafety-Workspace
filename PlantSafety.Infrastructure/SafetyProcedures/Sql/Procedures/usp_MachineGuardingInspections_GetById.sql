-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MachineGuardingInspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardingInspections_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single MachineGuardingInspections row by identifier, backing
--                  IMachineGuardingInspectionsRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MachineGuardingInspections_GetById]
(
    @MachineGuardingInspectionsId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardingInspectionsId], [TenantId], [SiteId], [Status], [InspectionTitle], [InspectionCategory], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [DeficiencyCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[MachineGuardingInspections]
    WHERE [MachineGuardingInspectionsId] = @MachineGuardingInspectionsId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

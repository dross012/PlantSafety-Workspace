-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.MachineGuardingInspections
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardingInspections_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted MachineGuardingInspections rows, backing
--                  IMachineGuardingInspectionsRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_MachineGuardingInspections_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardingInspectionsId], [TenantId], [SiteId], [Status], [InspectionTitle], [InspectionCategory], [EquipmentLocation], [InspectedByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [DeficiencyCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[MachineGuardingInspections]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

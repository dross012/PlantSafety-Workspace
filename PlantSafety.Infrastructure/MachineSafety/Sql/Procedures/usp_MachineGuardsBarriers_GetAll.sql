-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.MachineGuardsBarriers
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardsBarriers_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted MachineGuardsBarriers rows, backing
--                  IMachineGuardsBarriersRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_MachineGuardsBarriers_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardsBarriersId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentLocation], [InstalledByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [DeficiencyCount], [GuardIntegrityScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[MachineGuardsBarriers]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

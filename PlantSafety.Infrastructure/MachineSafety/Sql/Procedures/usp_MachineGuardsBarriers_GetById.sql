-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.MachineGuardsBarriers
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_MachineGuardsBarriers_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single MachineGuardsBarriers row by identifier, backing
--                  IMachineGuardsBarriersRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_MachineGuardsBarriers_GetById]
(
    @MachineGuardsBarriersId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [MachineGuardsBarriersId], [TenantId], [SiteId], [Status], [GuardTitle], [GuardType], [EquipmentLocation], [InstalledByUserId], [InspectedAtUtc], [NextInspectionDueAtUtc], [DeficiencyCount], [GuardIntegrityScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[MachineGuardsBarriers]
    WHERE [MachineGuardsBarriersId] = @MachineGuardsBarriersId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

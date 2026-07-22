-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.LotoPrograms
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoPrograms_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single LotoPrograms row by identifier, backing
--                  ILotoProgramsRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_LotoPrograms_GetById]
(
    @LotoProgramsId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoProgramsId], [TenantId], [SiteId], [Status], [ProgramTitle], [EquipmentTag], [EnergySourceTypes], [IsolationPointDescription], [IsolationPointCount], [AuthorizedByUserId], [LocksAppliedCount], [VerifiedByUserId], [VerifiedZeroEnergyAtUtc], [ReleasedByUserId], [ReleasedAtUtc], [LastAuditedAtUtc], [ProgramComplianceScore], [NonComplianceNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[LotoPrograms]
    WHERE [LotoProgramsId] = @LotoProgramsId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

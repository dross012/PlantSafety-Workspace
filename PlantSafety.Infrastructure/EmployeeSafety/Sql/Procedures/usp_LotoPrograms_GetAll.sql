-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.LotoPrograms
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoPrograms_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted LotoPrograms rows, backing
--                  ILotoProgramsRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_LotoPrograms_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoProgramsId], [TenantId], [SiteId], [Status], [ProgramTitle], [EquipmentTag], [EnergySourceTypes], [IsolationPointDescription], [IsolationPointCount], [AuthorizedByUserId], [LocksAppliedCount], [VerifiedByUserId], [VerifiedZeroEnergyAtUtc], [ReleasedByUserId], [ReleasedAtUtc], [LastAuditedAtUtc], [ProgramComplianceScore], [NonComplianceNotes], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[LotoPrograms]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

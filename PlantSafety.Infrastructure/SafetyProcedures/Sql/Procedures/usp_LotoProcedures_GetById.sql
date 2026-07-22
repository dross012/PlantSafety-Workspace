-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.LotoProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoProcedures_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single LotoProcedures row by identifier, backing
--                  ILotoProceduresRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_LotoProcedures_GetById]
(
    @LotoProceduresId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoProceduresId], [TenantId], [SiteId], [Status], [ProcedureTitle], [EnergySourceTypes], [IsolationPointDescription], [ApprovedByUserId], [PublishedAtUtc], [NextAuditDueAtUtc], [AuthorizedPersonnelCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[LotoProcedures]
    WHERE [LotoProceduresId] = @LotoProceduresId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

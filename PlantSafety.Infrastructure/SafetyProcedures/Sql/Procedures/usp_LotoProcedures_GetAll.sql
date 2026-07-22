-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.LotoProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LotoProcedures_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted LotoProcedures rows, backing
--                  ILotoProceduresRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_LotoProcedures_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [LotoProceduresId], [TenantId], [SiteId], [Status], [ProcedureTitle], [EnergySourceTypes], [IsolationPointDescription], [ApprovedByUserId], [PublishedAtUtc], [NextAuditDueAtUtc], [AuthorizedPersonnelCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[LotoProcedures]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

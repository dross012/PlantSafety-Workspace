-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.Ppe
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Ppe_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Ppe rows, backing
--                  IPpeRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_Ppe_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PpeId], [TenantId], [SiteId], [Status], [ItemDescription], [Category], [AssignedZone], [IssuedToUserId], [IssuedAtUtc], [RequiresFitTest], [FitTestedAtUtc], [ReplacementCycleDays], [NextReplacementDueAtUtc], [LastInspectedAtUtc], [ConditionScore], [ReplacementReason], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[Ppe]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

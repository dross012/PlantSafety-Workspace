-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.Ppe
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Ppe_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single Ppe row by identifier, backing
--                  IPpeRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_Ppe_GetById]
(
    @PpeId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [PpeId], [TenantId], [SiteId], [Status], [ItemDescription], [Category], [AssignedZone], [IssuedToUserId], [IssuedAtUtc], [RequiresFitTest], [FitTestedAtUtc], [ReplacementCycleDays], [NextReplacementDueAtUtc], [LastInspectedAtUtc], [ConditionScore], [ReplacementReason], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[Ppe]
    WHERE [PpeId] = @PpeId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

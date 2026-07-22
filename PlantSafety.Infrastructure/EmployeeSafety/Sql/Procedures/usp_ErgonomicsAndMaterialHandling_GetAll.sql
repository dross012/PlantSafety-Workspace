-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsAndMaterialHandling_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ErgonomicsAndMaterialHandling rows, backing
--                  IErgonomicsAndMaterialHandlingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_ErgonomicsAndMaterialHandling_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ErgonomicsAndMaterialHandlingId], [TenantId], [SiteId], [Status], [AssessmentTitle], [HazardCategory], [WorkstationLocation], [AssessedByUserId], [AssessedAtUtc], [RiskFactorCount], [RiskScore], [ControlsDescription], [FollowUpDueAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [EmployeeSafety].[ErgonomicsAndMaterialHandling]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

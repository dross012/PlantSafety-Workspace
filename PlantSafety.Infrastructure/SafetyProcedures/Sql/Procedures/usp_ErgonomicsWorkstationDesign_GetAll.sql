-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsWorkstationDesign_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ErgonomicsWorkstationDesign rows, backing
--                  IErgonomicsWorkstationDesignRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_ErgonomicsWorkstationDesign_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [ErgonomicsWorkstationDesignId], [TenantId], [SiteId], [Status], [ProcedureTitle], [DesignCategory], [WorkstationLocation], [ApprovedByUserId], [AssessedAtUtc], [NextAssessmentDueAtUtc], [RiskFactorCount], [ErgonomicsScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[ErgonomicsWorkstationDesign]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO

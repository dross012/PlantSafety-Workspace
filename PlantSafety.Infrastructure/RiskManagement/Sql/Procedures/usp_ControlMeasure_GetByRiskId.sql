-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ControlMeasure_GetByRiskId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all ControlMeasure child rows for a given RiskId. Additive-only evolution.
--  UPDATED:        2026-07-28 -- added ETag, projected from the RowVersion column:
--                  CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_ControlMeasure_GetByRiskId]
    @RiskId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT ControlMeasureId, RiskId, HierarchyLevel, Description, Justification, ResponsiblePersonId, ImplementedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [RiskMgmt].[ControlMeasure]
    WHERE RiskId = @RiskId AND IsDeleted = 0
    ORDER BY ImplementedAtUtc ASC;
END
GO

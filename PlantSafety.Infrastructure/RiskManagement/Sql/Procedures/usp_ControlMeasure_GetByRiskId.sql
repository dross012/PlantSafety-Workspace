-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ControlMeasure_GetByRiskId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all ControlMeasure child rows for a given RiskId. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_ControlMeasure_GetByRiskId]
    @RiskId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT ControlMeasureId, RiskId, HierarchyLevel, Description, Justification, ResponsiblePersonId, ImplementedAtUtc
    FROM [RiskMgmt].[ControlMeasure]
    WHERE RiskId = @RiskId AND IsDeleted = 0
    ORDER BY ImplementedAtUtc ASC;
END
GO

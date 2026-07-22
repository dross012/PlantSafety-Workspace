-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AutomatedIncidentDetection_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted AutomatedIncidentDetection rows. Backs IAutomatedIncidentDetectionRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_AutomatedIncidentDetection_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT AutomatedIncidentDetectionId, TenantId, SiteId, Status, DetectionType, LocationDescription, NotifiedSupervisorUserId, AcknowledgedBy, IncidentReferenceId, DismissalReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[AutomatedIncidentDetection]
    WHERE IsDeleted = 0;
END
GO

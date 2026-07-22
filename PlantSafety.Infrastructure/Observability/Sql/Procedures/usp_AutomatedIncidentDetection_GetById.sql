-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AutomatedIncidentDetection_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single AutomatedIncidentDetection row by identity. Backs IAutomatedIncidentDetectionRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_AutomatedIncidentDetection_GetById]
    @AutomatedIncidentDetectionId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT AutomatedIncidentDetectionId, TenantId, SiteId, Status, DetectionType, LocationDescription, NotifiedSupervisorUserId, AcknowledgedBy, IncidentReferenceId, DismissalReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Observability].[AutomatedIncidentDetection]
    WHERE AutomatedIncidentDetectionId = @AutomatedIncidentDetectionId AND IsDeleted = 0;
END
GO

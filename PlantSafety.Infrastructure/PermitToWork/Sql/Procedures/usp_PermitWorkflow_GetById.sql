-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitWorkflow_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PermitWorkflow row by identity. Backs IPermitWorkflowRepository.GetByIdAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-29 -- Projects RowVersion as a string ETag (Gap 2 remediation),
--                  matching the ProcessManagement.usp_Process_GetById /
--                  SafetyEvents.usp_Incident_Update CONVERT(NVARCHAR(34), RowVersion, 1)
--                  convention, so callers can capture the current concurrency token for
--                  read-before-write flows.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitWorkflow_GetById]
    @PermitWorkflowId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitWorkflowId, TenantId, SiteId, Status, PermitNumber, PermitTypeCode, RequesterUserId, IssuerUserId, WorkDescription, RequiredControls, RiskScore, ControlsConfirmedBy, ControlsImplementedAtUtc, ApprovedBy, ApprovedAtUtc, IssuedAtUtc, ExpiresAtUtc, ClosureSignedBy, ClosureNotes, ClosedAtUtc, ArchivedAtUtc, RejectedReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PermitToWork].[PermitWorkflow]
    WHERE PermitWorkflowId = @PermitWorkflowId AND IsDeleted = 0;
END
GO

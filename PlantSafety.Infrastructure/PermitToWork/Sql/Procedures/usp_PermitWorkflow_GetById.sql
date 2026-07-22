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
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitWorkflow_GetById]
    @PermitWorkflowId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitWorkflowId, TenantId, SiteId, Status, PermitNumber, PermitTypeCode, RequesterUserId, IssuerUserId, WorkDescription, RequiredControls, RiskScore, ControlsConfirmedBy, ControlsImplementedAtUtc, ApprovedBy, ApprovedAtUtc, IssuedAtUtc, ExpiresAtUtc, ClosureSignedBy, ClosureNotes, ClosedAtUtc, ArchivedAtUtc, RejectedReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PermitToWork].[PermitWorkflow]
    WHERE PermitWorkflowId = @PermitWorkflowId AND IsDeleted = 0;
END
GO

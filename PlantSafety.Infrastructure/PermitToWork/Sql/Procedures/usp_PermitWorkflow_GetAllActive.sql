-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitWorkflow_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PermitWorkflow rows. Backs IPermitWorkflowRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitWorkflow_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitWorkflowId, TenantId, SiteId, Status, PermitNumber, PermitTypeCode, RequesterUserId, IssuerUserId, WorkDescription, RequiredControls, RiskScore, ControlsConfirmedBy, ControlsImplementedAtUtc, ApprovedBy, ApprovedAtUtc, IssuedAtUtc, ExpiresAtUtc, ClosureSignedBy, ClosureNotes, ClosedAtUtc, ArchivedAtUtc, RejectedReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PermitToWork].[PermitWorkflow]
    WHERE IsDeleted = 0;
END
GO

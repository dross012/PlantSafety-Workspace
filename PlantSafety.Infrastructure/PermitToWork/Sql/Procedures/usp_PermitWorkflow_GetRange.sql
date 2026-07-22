-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitWorkflow_GetRange.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPermitWorkflowRepository.GetPermitWorkflowAsync
--                  (Application-layer read model). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitWorkflow_GetRange]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitWorkflowId, TenantId, SiteId, Status, PermitNumber, PermitTypeCode, RequesterUserId, IssuerUserId, WorkDescription, CreatedAtUtc
    FROM [PermitToWork].[PermitWorkflow]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO

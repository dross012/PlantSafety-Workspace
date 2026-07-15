-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Policy.Approvals
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_PolicyApprovals_GetApprovals.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IApprovalsRepository.GetApprovalsAsync (Policy variant).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_PolicyApprovals_GetApprovals]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ApprovalId,
        TenantId,
        SiteId,
        PolicyId,
        PolicyTitle,
        Status,
        DecisionAtUtc,
        ApproverId
    FROM [Compliance].[PolicyApprovals]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DecisionAtUtc >= @FromUtc
      AND DecisionAtUtc <= @ToUtc
    ORDER BY DecisionAtUtc DESC;
END
GO

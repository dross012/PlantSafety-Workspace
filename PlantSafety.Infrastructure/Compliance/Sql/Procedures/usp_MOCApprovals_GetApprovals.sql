-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.Approvals
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_MOCApprovals_GetApprovals.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IApprovalsRepository.GetApprovalsAsync (ManagementOfChange variant).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCApprovals_GetApprovals]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ApprovalId,
        ChangeRequestId,
        TenantId,
        SiteId,
        ApproverName,
        Status,
        DecidedAtUtc,
        ApprovedBy
    FROM [Compliance].[MOCApprovals]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DecidedAtUtc >= @FromUtc
      AND DecidedAtUtc <= @ToUtc
    ORDER BY DecidedAtUtc DESC;
END
GO

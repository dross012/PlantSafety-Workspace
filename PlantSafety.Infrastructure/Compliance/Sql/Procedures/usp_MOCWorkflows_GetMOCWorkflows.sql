-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.MOCWorkflows
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_MOCWorkflows_GetMOCWorkflows.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IMOCWorkflowsRepository.GetMOCWorkflowsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCWorkflows_GetMOCWorkflows]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        WorkflowId,
        TenantId,
        SiteId,
        ChangeTitle,
        CurrentStage,
        Status,
        InitiatedAtUtc,
        InitiatedBy
    FROM [Compliance].[MOCWorkflows]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND InitiatedAtUtc >= @FromUtc
      AND InitiatedAtUtc <= @ToUtc
    ORDER BY InitiatedAtUtc DESC;
END
GO

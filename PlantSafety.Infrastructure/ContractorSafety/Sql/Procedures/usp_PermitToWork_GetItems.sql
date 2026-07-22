-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitToWork_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IPermitToWorkRepository.GetPermitToWorkItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PermitToWork_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [PermitToWorkId],
        [TenantId],
        [SiteId],
        [PermitTitle],
        [PermitType] AS [PermitCategory],
        [WorkLocation],
        [Status] AS [PermitStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [ContractorSafety].[PermitToWork]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

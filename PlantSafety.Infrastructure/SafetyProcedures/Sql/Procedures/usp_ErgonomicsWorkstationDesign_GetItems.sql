-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsWorkstationDesign_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IErgonomicsWorkstationDesignRepository.GetErgonomicsWorkstationDesignItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_ErgonomicsWorkstationDesign_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [ErgonomicsWorkstationDesignId],
        [TenantId],
        [SiteId],
        [ProcedureTitle],
        [DesignCategory] AS [DesignCategory],
        [WorkstationLocation],
        [Status] AS [ProcedureStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [SafetyProcedures].[ErgonomicsWorkstationDesign]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO

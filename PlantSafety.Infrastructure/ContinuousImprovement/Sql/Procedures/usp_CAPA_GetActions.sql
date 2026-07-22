-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CAPA_GetActions.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ICAPARepository.GetActionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CAPA_GetActions]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CapaId,
        TenantId,
        SiteId,
        ActionType,
        Title,
        Status,
        DueDateUtc,
        AssignedTo
    FROM [ContinuousImprovement].[CAPA]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND DueDateUtc >= @FromUtc
      AND DueDateUtc <= @ToUtc
    ORDER BY DueDateUtc DESC;
END
GO

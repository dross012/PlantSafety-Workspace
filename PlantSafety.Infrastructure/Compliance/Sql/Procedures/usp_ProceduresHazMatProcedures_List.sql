-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresHazMatProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ProceduresHazMatProcedures_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[ProceduresHazMatProcedures],
--                  backing IHazMatProceduresRepository.GetHazMatProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresHazMatProcedures_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ProcedureId],
        [TenantId],
        [SiteId],
        [HazardousMaterial],
        [HandlingStep],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresHazMatProcedures]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO
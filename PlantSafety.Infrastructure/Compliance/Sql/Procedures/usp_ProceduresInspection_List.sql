-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresInspection
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ProceduresInspection_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[ProceduresInspection],
--                  backing IInspectionRepository.GetInspectionProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresInspection_List]
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
        [InspectionType],
        [Frequency],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresInspection]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO
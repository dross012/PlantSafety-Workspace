-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresWorkInstructions
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ProceduresWorkInstructions_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Compliance].[ProceduresWorkInstructions],
--                  backing IWorkInstructionsRepository.GetWorkInstructionsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_ProceduresWorkInstructions_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [InstructionId],
        [TenantId],
        [SiteId],
        [Title],
        [WorkArea],
        [Status],
        [EffectiveAtUtc],
        [OwnerId]
FROM [Compliance].[ProceduresWorkInstructions]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [EffectiveAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [EffectiveAtUtc] DESC;
GO
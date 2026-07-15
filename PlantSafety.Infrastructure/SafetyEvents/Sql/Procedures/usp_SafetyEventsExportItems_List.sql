-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.SafetyEventsExportItems
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_SafetyEventsExportItems_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, filtered, row-capped list procedure for
--                  [SafetyEvents].[SafetyEventsExportItems], backing
--                  IExportSafetyEventsRepository.GenerateExportAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_SafetyEventsExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
    @Severity       NVARCHAR(50) = NULL,
    @MaxRecords     INT
AS
SET NOCOUNT ON;

SELECT TOP (@MaxRecords)
        [Id],
        [TenantId],
        [SiteId],
        [ResourceType],
        [ResourceId],
        [Title],
        [Status],
        [Severity],
        [EffectiveAtUtc],
        [OwnerId]
FROM [SafetyEvents].[SafetyEventsExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
  AND (@Severity IS NULL OR [Severity] = @Severity)
ORDER BY [EffectiveAtUtc] DESC;
GO

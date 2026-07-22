-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ExportItems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperationalExportItems_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, filtered, row-capped list procedure for
--                  [Operational].[OperationalExportItems], backing
--                  IExportOperationalRepository.GenerateExportAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_OperationalExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
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
        [EffectiveAtUtc],
        [OwnerId]
FROM [Operational].[OperationalExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
ORDER BY [EffectiveAtUtc] DESC;
GO

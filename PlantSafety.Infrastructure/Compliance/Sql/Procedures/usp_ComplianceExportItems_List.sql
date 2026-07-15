-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ComplianceExportItems
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ComplianceExportItems_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, filtered, row-capped list procedure for
--                  [Compliance].[ComplianceExportItems], backing
--                  IExportComplianceRepository.GenerateExportAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_ComplianceExportItems_List]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @FromUtc        DATETIME2(3) = NULL,
    @ToUtc          DATETIME2(3),
    @Status         NVARCHAR(50) = NULL,
    @ResourceType   NVARCHAR(100) = NULL,
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
FROM [Compliance].[ComplianceExportItems]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND (@FromUtc IS NULL OR [EffectiveAtUtc] >= @FromUtc)
  AND [EffectiveAtUtc] <= @ToUtc
  AND (@Status IS NULL OR [Status] = @Status)
  AND (@ResourceType IS NULL OR [ResourceType] = @ResourceType)
ORDER BY [EffectiveAtUtc] DESC;
GO

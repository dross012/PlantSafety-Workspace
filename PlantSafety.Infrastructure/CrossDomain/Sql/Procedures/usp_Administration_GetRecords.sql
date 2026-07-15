-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Administration
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Administration_GetRecords.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Administration],
--                  backing IAdministrationRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Administration_GetRecords]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RecordId],
        [TenantId],
        [SiteId],
        [SettingKey],
        [SettingValue],
        [Category],
        [Status],
        [LastModifiedAtUtc],
        [LastModifiedBy]
FROM [CrossDomain].[Administration]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastModifiedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastModifiedAtUtc] DESC;
GO

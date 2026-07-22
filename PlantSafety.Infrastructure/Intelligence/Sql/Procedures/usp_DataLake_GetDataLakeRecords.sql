-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.DataLake
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DataLake_GetDataLakeRecords.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IDataLakeRepository.GetDataLakeRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Intelligence].[usp_DataLake_GetDataLakeRecords]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        DataLakeRecordId,
        TenantId,
        SiteId,
        DatasetName,
        Zone,
        RecordCount,
        Status,
        ProcessedAtUtc,
        ProcessedBy
    FROM [Intelligence].[DataLake]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ProcessedAtUtc >= @FromUtc
      AND ProcessedAtUtc <= @ToUtc
    ORDER BY ProcessedAtUtc DESC;
END
GO

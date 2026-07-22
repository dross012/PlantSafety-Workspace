-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Financials.Financials
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Financials_GetRecords.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IFinancialsRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Financials].[usp_Financials_GetRecords]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RecordId,
        TenantId,
        SiteId,
        Category,
        Amount,
        Status,
        RecordedAtUtc,
        RecordedBy
    FROM [Financials].[Financials]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND RecordedAtUtc >= @FromUtc
      AND RecordedAtUtc <= @ToUtc
    ORDER BY RecordedAtUtc DESC;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RegulatoryCalendar
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RegulatoryCalendar_GetRange.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRegulatoryCalendarRepository.GetRegulatoryCalendarAsync (Application-layer
--                  read model / GetRegulatoryCalendarQuery). Projects RowVersion as a string ETag
--                  so RegulatoryCalendarItemDto.ETag can round-trip back into a subsequent
--                  mutating command's IConcurrencyAwareCommand.ETag for read-before-write clients.
--                  Mirrors usp_Permits_GetRange.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_RegulatoryCalendar_GetRange]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RegulatoryCalendarId, TenantId, SiteId, Status, EventTitle, RegulatoryBody, JurisdictionCode, ComplianceCategory,
        ResponsiblePartyId, DueAtUtc, ReminderLeadDays, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[RegulatoryCalendar]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO

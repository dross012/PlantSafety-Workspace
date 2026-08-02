-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RegulatoryCalendar
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RegulatoryCalendar_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted RegulatoryCalendar rows. Backs
--                  IRegulatoryCalendarRepository.ListAsync, whose RegulatoryCalendarSpecification
--                  predicate is applied in-memory over the candidate set returned here (the
--                  Domain interface carries no tenant/site scoping parameter to push down to
--                  SQL). Mirrors usp_Permits_GetAllActive.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_RegulatoryCalendar_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RegulatoryCalendarId, TenantId, SiteId, Status, EventTitle, RegulatoryBody, JurisdictionCode, ComplianceCategory,
        ResponsiblePartyId, DueAtUtc, ReminderLeadDays, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[RegulatoryCalendar]
    WHERE IsDeleted = 0;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Incidents
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PHMSAIncidentReport_GetAllActive.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PHMSA_IncidentReport rows, ordered by CreatedAtUtc.
--                  Backs IIncidentReportRepository.ListAsync, mirroring usp_Permits_GetAllActive.sql
--                  (the Domain interface carries no tenant/site scoping parameter to push down to
--                  SQL, so filtering is applied in-memory over the candidate set returned here, same
--                  as the Permits precedent). Optional @FromUtc/@ToUtc filter against
--                  IncidentDateUtc and optional @IsReportableToPhmsa filter support an incident
--                  listing screen that commonly needs a date range and reportability filter.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_PHMSAIncidentReport_GetAllActive]
    @FromUtc DATETIME2(3) = NULL,
    @ToUtc DATETIME2(3) = NULL,
    @IsReportableToPhmsa BIT = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        IncidentReportId, TenantId, SiteId, Status, IncidentType, MaterialInvolvedId, MaterialInvolvedDescription,
        Injuries, FatalitiesInvolved, PropertyDamageEstimateUsd, Narrative, IncidentDateUtc, Location,
        IsReportableToPhmsa,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Compliance].[PHMSA_IncidentReport]
    WHERE IsDeleted = 0
      AND (@FromUtc IS NULL OR IncidentDateUtc >= @FromUtc)
      AND (@ToUtc IS NULL OR IncidentDateUtc <= @ToUtc)
      AND (@IsReportableToPhmsa IS NULL OR IsReportableToPhmsa = @IsReportableToPhmsa)
    ORDER BY CreatedAtUtc;
END
GO

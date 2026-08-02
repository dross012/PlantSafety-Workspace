-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EmergencyManagementItems_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EmergencyManagementItems rows. Backs
--                  IEmergencyManagementRepository.ListAsync (Domain write-side repository),
--                  whose EmergencyManagementSpecification predicate is applied in-memory over the
--                  candidate set returned here (the Domain interface carries no tenant/site
--                  scoping parameter to push down to SQL). Mirrors usp_Permits_GetAllActive.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_EmergencyManagementItems_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        EmergencyId, TenantId, SiteId, Status, EmergencyType, Severity, Location, CasualtyCount,
        ResponseTeamNotified, ReportedAtUtc, StabilizedAtUtc, ResolvedAtUtc, ReportedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [SafetyEvents].[EmergencyManagementItems]
    WHERE IsDeleted = 0;
END
GO

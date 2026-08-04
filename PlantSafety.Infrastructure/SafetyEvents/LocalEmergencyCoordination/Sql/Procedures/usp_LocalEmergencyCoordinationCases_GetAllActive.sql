-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_LocalEmergencyCoordinationCases_GetAllActive.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted LocalEmergencyCoordinationCases case rows (case-level
--                  fields only). Backs ILocalEmergencyCoordinationRepository.ListAsync (Domain
--                  write-side), whose LocalEmergencyCoordinationSpecification predicate is applied
--                  in-memory over the candidate set returned here. Mirrors
--                  usp_EmergencyManagementItems_GetAllActive.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_LocalEmergencyCoordinationCases_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        LocalEmergencyCoordinationId, TenantId, SiteId, IncidentId, CaseName, Status, CreatedAtUtc, CreatedBy, ClosedAtUtc, ArchivedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [SafetyEvents].[LocalEmergencyCoordinationCases]
    WHERE IsDeleted = 0;
END
GO

-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_LiaisonLogEntries_GetByIncident.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  ILocalEmergencyCoordinationRepository.GetLiaisonLogByIncidentAsync
--                  (Application read-model).
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_LiaisonLogEntries_GetByIncident]
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT LiaisonLogEntryId, ExternalResponderLiaisonId, LocalEmergencyCoordinationId, IncidentId, AgencyName, Message, LoggedAtUtc, LoggedBy
    FROM [SafetyEvents].[LiaisonLogEntries]
    WHERE TenantId = @TenantId AND IncidentId = @IncidentId
    ORDER BY LoggedAtUtc DESC;
END
GO

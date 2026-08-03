-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentTimelineEntry_Count.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns the total timeline entry count for a single incident, for pagination
--                  metadata. Backs IIncidentTimelineReadRepository.ListAsync /
--                  IncidentTimelineReadDapperRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentTimelineEntry_Count]
(
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT COUNT(*)
        FROM [SafetyEvents].[IncidentTimelineEntry]
        WHERE TenantId = @TenantId
          AND IncidentId = @IncidentId;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

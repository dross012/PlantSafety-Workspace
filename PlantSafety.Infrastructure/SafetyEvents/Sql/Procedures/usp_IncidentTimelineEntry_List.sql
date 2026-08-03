-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentTimelineEntry_List.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, chronologically-ordered (oldest first) list of
--                  [SafetyEvents].[IncidentTimelineEntry] rows for a single incident. Backs
--                  IIncidentTimelineReadRepository.ListAsync / IncidentTimelineReadDapperRepository.
--                  ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentTimelineEntry_List]
(
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize   INT = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            EntryId,
            TimestampUtc,
            Actor,
            EventType,
            Severity,
            Description
        FROM [SafetyEvents].[IncidentTimelineEntry]
        WHERE TenantId = @TenantId
          AND IncidentId = @IncidentId
        ORDER BY TimestampUtc ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO

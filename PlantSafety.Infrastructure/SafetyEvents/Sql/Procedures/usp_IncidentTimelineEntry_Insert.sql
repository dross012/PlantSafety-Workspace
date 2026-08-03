-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentTimelineEntry_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a single row into [SafetyEvents].[IncidentTimelineEntry]. Backs
--                  IncidentDapperRepository.PersistPendingTimelineEntriesAsync, called once per
--                  pending IncidentManagementTimelineEntry after each lifecycle command commits.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentTimelineEntry_Insert]
    @EntryId      UNIQUEIDENTIFIER,
    @IncidentId   UNIQUEIDENTIFIER,
    @TenantId     UNIQUEIDENTIFIER,
    @TimestampUtc DATETIME2(3),
    @Actor        NVARCHAR(200),
    @EventType    NVARCHAR(30),
    @Severity     NVARCHAR(30),
    @Description  NVARCHAR(2000)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[IncidentTimelineEntry]
            (EntryId, IncidentId, TenantId, TimestampUtc, Actor, EventType, Severity, Description)
        VALUES
            (@EntryId, @IncidentId, @TenantId, @TimestampUtc, @Actor, @EventType, @Severity, @Description);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; EntryId=', @EntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO

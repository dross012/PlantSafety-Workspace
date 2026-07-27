-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Incident_Create.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [SafetyEvents].[Incident] row. Backs
--                  IIncidentRepository.CreateAsync / IncidentDapperRepository.CreateAsync.
--                  Parameter set matches StoredProcedureBuilder.WithCanonicalHeaders
--                  (@TenantId/@CorrelationId/@ActorId/@NowUtc) plus the explicit Add* calls in
--                  IncidentDapperRepository.CreateAsync. @NowUtc (app-supplied) is used for all
--                  business timestamp columns instead of SYSUTCDATETIME() to keep the write
--                  deterministic/replayable; SYSUTCDATETIME() is retained only for the
--                  best-effort AuditLog/ErrorLog metadata timestamps, matching the established
--                  usp_Accident_Insert / usp_CorrectiveAction_Upsert precedent for that
--                  distinction. TRY/CATCH with AuditDB/ErrorDB logging per the pattern already
--                  established by usp_Accident_Insert. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_Create]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @IncidentId             UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER,
    @Title                  NVARCHAR(200),
    @Description            NVARCHAR(4000),
    @IncidentOccurredAtUtc  DATETIME2(3),
    @Severity               NVARCHAR(30),
    @Location               NVARCHAR(500),
    @ReportedByWorkerId     UNIQUEIDENTIFIER,
    @AssignedInvestigatorId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [SafetyEvents].[Incident]
        (
            IncidentId, TenantId, SiteId, Title, Description, IncidentOccurredAtUtc, ReportedAtUtc,
            Severity, Status, Location, ReportedByWorkerId, AssignedInvestigatorId, ClosedAtUtc,
            DeletionReason, DeletedAtUtc, IsDeleted, LastModifiedByActorId, LastModifiedAtUtc,
            CreatedAtUtc, CreatedBy
        )
        VALUES
        (
            @IncidentId, @TenantId, @SiteId, @Title, @Description, @IncidentOccurredAtUtc, @NowUtc,
            @Severity, N'Reported', @Location, @ReportedByWorkerId, @AssignedInvestigatorId, NULL,
            NULL, NULL, 0, @ActorId, @NowUtc,
            @NowUtc, @ActorId
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Incident', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

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
